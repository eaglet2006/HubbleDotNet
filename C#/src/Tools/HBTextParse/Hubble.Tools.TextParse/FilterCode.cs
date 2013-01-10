///
/// Sample library for using IFilter to read text from any registered filter type.
/// 
///  Helpful links:
///     http://msdn.microsoft.com/en-us/library/ms691105(VS.85).aspx
///     http://ifilter.codeplex.com/
///     http://www.pinvoke.net/default.aspx/query/LoadIFilter.html
///     
///  Code here is taken from a combination of the project located at http://ifilter.codeplex.com/
///  as well as definitions taken from p-invoke.net.  License is MS-PL so enjoy.
/// 
///  Modify by eaglet at 2013-01-09, add convert to file method


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace FilterLibrary
{
    public class FilterCode
    {
        int DefaultBufferSize = 4096;

        /// <summary>
        /// Utilizes IFilter interface in Windows to parse the contents of files.
        /// </summary>
        /// <param name="path">Path - Location of file to parse</param>
        /// <param name="Path">Buffer - Return text artifacts</param>
        /// <returns>Raw set of strings from the document in plain text format.</returns>
        public void ConvertToFile(string path, string outuptFilePath, Encoding encoding)
        {
            IFilter filter = null;
            int hresult;
            IFilterReturnCodes rtn;

            // Try to load the filter for the path given.
            hresult = LoadIFilter(path, new IntPtr(0), ref filter);
            if (hresult == 0)
            {
                using (StreamWriter outFile = new StreamWriter(outuptFilePath, false, encoding))
                {
                    IFILTER_FLAGS uflags;

                    // Init the filter provider.
                    rtn = filter.Init(
                            IFILTER_INIT.IFILTER_INIT_CANON_PARAGRAPHS |
                            IFILTER_INIT.IFILTER_INIT_CANON_HYPHENS |
                            IFILTER_INIT.IFILTER_INIT_CANON_SPACES |
                            IFILTER_INIT.IFILTER_INIT_APPLY_INDEX_ATTRIBUTES |
                            IFILTER_INIT.IFILTER_INIT_INDEXING_ONLY,
                            0, new IntPtr(0), out uflags);
                    if (rtn == IFilterReturnCodes.S_OK)
                    {
                        STAT_CHUNK statChunk;

                        // Outer loop will read chunks from the document at a time.  For those
                        // chunks that have text, the contents will be pulled and put into the
                        // return buffer.
                        bool bMoreChunks = true;
                        while (bMoreChunks)
                        {
                            rtn = filter.GetChunk(out statChunk);
                            if (rtn == IFilterReturnCodes.S_OK)
                            {
                                // Ignore all non-text chunks.
                                if (statChunk.flags != CHUNKSTATE.CHUNK_TEXT)
                                    continue;

                                // Check for white space items and add the appropriate breaks.
                                switch (statChunk.breakType)
                                {
                                    case CHUNK_BREAKTYPE.CHUNK_NO_BREAK:
                                        break;

                                    case CHUNK_BREAKTYPE.CHUNK_EOW:
                                        outFile.Write(' ');
                                        break;

                                    case CHUNK_BREAKTYPE.CHUNK_EOC:
                                    case CHUNK_BREAKTYPE.CHUNK_EOP:
                                    case CHUNK_BREAKTYPE.CHUNK_EOS:
                                        outFile.WriteLine();

                                        break;
                                }

                                // At this point we have a text chunk.  The following code will pull out
                                // all of it and add it to the buffer.
                                bool bMoreText = true;
                                while (bMoreText)
                                {
                                    // Create a temporary string buffer we can use for the parsing algorithm.
                                    int cBuffer = DefaultBufferSize;
                                    StringBuilder sbBuffer = new StringBuilder(DefaultBufferSize);

                                    // Read the next piece of data up to the size of our local buffer.
                                    rtn = filter.GetText(ref cBuffer, sbBuffer);
                                    if (rtn == IFilterReturnCodes.S_OK || rtn == IFilterReturnCodes.FILTER_S_LAST_TEXT)
                                    {
                                        // If any data was returned, scrub it and then add it to the buffer.
                                        CleanUpCharacters(cBuffer, sbBuffer);
                                        outFile.Write(sbBuffer.ToString());

                                        // If we got back some text but there is no more, terminate the loop.
                                        if (rtn == IFilterReturnCodes.FILTER_S_LAST_TEXT)
                                        {
                                            bMoreText = false;
                                            break;
                                        }
                                    }
                                    // Once all data is exhausted, we are done so terminate.
                                    else if (rtn == IFilterReturnCodes.FILTER_E_NO_MORE_TEXT)
                                    {
                                        bMoreText = false;
                                        break;
                                    }
                                    // Check for any fatal errors.  It is a bug if you land here.
                                    else if (rtn == IFilterReturnCodes.FILTER_E_NO_TEXT)
                                    {
                                        System.Diagnostics.Debug.Assert(false, "Should not get here");
                                        throw new InvalidOperationException();
                                    }
                                }
                            }
                            // Once all chunks have been read, we are done with the file.
                            else if (rtn == IFilterReturnCodes.FILTER_E_END_OF_CHUNKS)
                            {
                                bMoreChunks = false;
                                break;
                            }
                            else if (rtn == IFilterReturnCodes.FILTER_E_EMBEDDING_UNAVAILABLE ||
                                rtn == IFilterReturnCodes.FILTER_E_LINK_UNAVAILABLE)
                            {
                                continue;
                            }
                            else
                            {
                                throw new COMException("IFilter COM error: " + rtn.ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                // If you get here there is no filter for the file type you asked for.  Throw an
                // exception for the caller.
                throw new InvalidOperationException("Failed to find IFilter for file " + path);
            }

        }


        /// <summary>
        /// Utilizes IFilter interface in Windows to parse the contents of files.
        /// </summary>
        /// <param name="path">Path - Location of file to parse</param>
        /// <param name="buffer">Buffer - Return text artifacts</param>
        /// <returns>Raw set of strings from the document in plain text format.</returns>
        public void GetTextFromDocument(string path, ref StringBuilder buffer)
        {
            IFilter filter = null;
            int hresult;
            IFilterReturnCodes rtn;

            // Initialize the return buffer to 64K.
            buffer = new StringBuilder(64 * 1024);

            // Try to load the filter for the path given.
            hresult = LoadIFilter(path, new IntPtr(0), ref filter);
            if (hresult == 0)
            {
                IFILTER_FLAGS uflags;

                // Init the filter provider.
                rtn = filter.Init(
                        IFILTER_INIT.IFILTER_INIT_CANON_PARAGRAPHS |
                        IFILTER_INIT.IFILTER_INIT_CANON_HYPHENS |
                        IFILTER_INIT.IFILTER_INIT_CANON_SPACES |
                        IFILTER_INIT.IFILTER_INIT_APPLY_INDEX_ATTRIBUTES |
                        IFILTER_INIT.IFILTER_INIT_INDEXING_ONLY,
                        0, new IntPtr(0), out uflags);
                if (rtn == IFilterReturnCodes.S_OK)
                {
                    STAT_CHUNK statChunk;

                    // Outer loop will read chunks from the document at a time.  For those
                    // chunks that have text, the contents will be pulled and put into the
                    // return buffer.
                    bool bMoreChunks = true;
                    while (bMoreChunks)
                    {
                        rtn = filter.GetChunk(out statChunk);
                        if (rtn == IFilterReturnCodes.S_OK)
                        {
                            // Ignore all non-text chunks.
                            if (statChunk.flags != CHUNKSTATE.CHUNK_TEXT)
                                continue;

                            // Check for white space items and add the appropriate breaks.
                            switch (statChunk.breakType)
                            {
                                case CHUNK_BREAKTYPE.CHUNK_NO_BREAK:
                                    break;

                                case CHUNK_BREAKTYPE.CHUNK_EOW:
                                    buffer.Append(' ');
                                    break;

                                case CHUNK_BREAKTYPE.CHUNK_EOC:
                                case CHUNK_BREAKTYPE.CHUNK_EOP:
                                case CHUNK_BREAKTYPE.CHUNK_EOS:
                                    buffer.AppendLine();
                                    break;
                            }

                            // At this point we have a text chunk.  The following code will pull out
                            // all of it and add it to the buffer.
                            bool bMoreText = true;
                            while (bMoreText)
                            {
                                // Create a temporary string buffer we can use for the parsing algorithm.
                                int cBuffer = DefaultBufferSize;
                                StringBuilder sbBuffer = new StringBuilder(DefaultBufferSize);

                                // Read the next piece of data up to the size of our local buffer.
                                rtn = filter.GetText(ref cBuffer, sbBuffer);
                                if (rtn == IFilterReturnCodes.S_OK || rtn == IFilterReturnCodes.FILTER_S_LAST_TEXT)
                                {
                                    // If any data was returned, scrub it and then add it to the buffer.
                                    CleanUpCharacters(cBuffer, sbBuffer);
                                    buffer.Append(sbBuffer.ToString());

                                    // If we got back some text but there is no more, terminate the loop.
                                    if (rtn == IFilterReturnCodes.FILTER_S_LAST_TEXT)
                                    {
                                        bMoreText = false;
                                        break;
                                    }
                                }
                                // Once all data is exhausted, we are done so terminate.
                                else if (rtn == IFilterReturnCodes.FILTER_E_NO_MORE_TEXT)
                                {
                                    bMoreText = false;
                                    break;
                                }
                                // Check for any fatal errors.  It is a bug if you land here.
                                else if (rtn == IFilterReturnCodes.FILTER_E_NO_TEXT)
                                {
                                    System.Diagnostics.Debug.Assert(false, "Should not get here");
                                    throw new InvalidOperationException();
                                }
                            }
                        }
                        // Once all chunks have been read, we are done with the file.
                        else if (rtn == IFilterReturnCodes.FILTER_E_END_OF_CHUNKS)
                        {
                            bMoreChunks = false;
                            break;
                        }
                        else if (rtn == IFilterReturnCodes.FILTER_E_EMBEDDING_UNAVAILABLE ||
                            rtn == IFilterReturnCodes.FILTER_E_LINK_UNAVAILABLE)
                        {
                            continue;
                        }
                        else
                        {
                            throw new COMException("IFilter COM error: " + rtn.ToString());
                        }
                    }
                }
            }
            else
            {
                // If you get here there is no filter for the file type you asked for.  Throw an
                // exception for the caller.
                throw new InvalidOperationException("Failed to find IFilter for file " + path);
            }
        }

        [DllImport("query.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int LoadIFilter(string pwcsPath,
                  [MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
                  ref IFilter ppIUnk);

        [ComImport, Guid("89BCB740-6119-101A-BCB7-00DD010655AF")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IFilter
        {
            /// <summary>
            /// The IFilter::Init method initializes a filtering session.
            /// </summary>
            [PreserveSig]
            IFilterReturnCodes Init(
                //[in] Flag settings from the IFILTER_INIT enumeration for
                // controlling text standardization, property output, embedding
                // scope, and IFilter access patterns. 
              IFILTER_INIT grfFlags,

              // [in] The size of the attributes array. When nonzero, cAttributes
                //  takes 
                // precedence over attributes specified in grfFlags. If no
                // attribute flags 
                // are specified and cAttributes is zero, the default is given by
                // the 
                // PSGUID_STORAGE storage property set, which contains the date and
                //  time 
                // of the last write to the file, size, and so on; and by the
                //  PID_STG_CONTENTS 
                // 'contents' property, which maps to the main contents of the
                // file. 
                // For more information about properties and property sets, see
                // Property Sets. 
              int cAttributes,

              //[in] Array of pointers to FULLPROPSPEC structures for the
                // requested properties. 
                // When cAttributes is nonzero, only the properties in aAttributes
                // are returned. 
              IntPtr aAttributes,

              // [out] Information about additional properties available to the
                //  caller; from the IFILTER_FLAGS enumeration. 
              out IFILTER_FLAGS pdwFlags);

            /// <summary>
            /// The IFilter::GetChunk method positions the filter at the beginning
            /// of the next chunk, 
            /// or at the first chunk if this is the first call to the GetChunk
            /// method, and returns a description of the current chunk. 
            /// </summary>
            [PreserveSig]
            IFilterReturnCodes GetChunk(out STAT_CHUNK pStat);

            /// <summary>
            /// The IFilter::GetText method retrieves text (text-type properties)
            /// from the current chunk, 
            /// which must have a CHUNKSTATE enumeration value of CHUNK_TEXT.
            /// </summary>
            [PreserveSig]
            IFilterReturnCodes GetText(
                // [in/out] On entry, the size of awcBuffer array in wide/Unicode
                // characters. On exit, the number of Unicode characters written to
                // awcBuffer. 
                // Note that this value is not the number of bytes in the buffer. 
                ref int pcwcBuffer,

                // Text retrieved from the current chunk. Do not terminate the
                // buffer with a character.  
                [Out(), MarshalAs(UnmanagedType.LPWStr)] 
       StringBuilder awcBuffer);

            /// <summary>
            /// The IFilter::GetValue method retrieves a value (public
            /// value-type property) from a chunk, 
            /// which must have a CHUNKSTATE enumeration value of CHUNK_VALUE.
            /// </summary>
            [PreserveSig]
            IFilterReturnCodes GetValue(
                // Allocate the PROPVARIANT structure with CoTaskMemAlloc. Some
                // PROPVARIANT 
                // structures contain pointers, which can be freed by calling the
                // PropVariantClear function. 
                // It is up to the caller of the GetValue method to call the
                //   PropVariantClear method.            
                // ref IntPtr ppPropValue
                // [MarshalAs(UnmanagedType.Struct)]
                ref IntPtr PropVal);

            /// <summary>
            /// The IFilter::BindRegion method retrieves an interface representing
            /// the specified portion of the object. 
            /// Currently reserved for future use.
            /// </summary>
            [PreserveSig]
            IFilterReturnCodes BindRegion(ref FILTERREGION origPos,
              ref Guid riid, ref object ppunk);
        }

        public struct FILTERREGION
        {
            public int idChunk;
            public int cwcStart;
            public int cwcExtent;
        }

        public enum IFilterReturnCodes : uint
        {
            /// <summary>
            /// Success
            /// </summary>
            S_OK = 0,
            /// <summary>
            /// The function was denied access to the filter file. 
            /// </summary>
            E_ACCESSDENIED = 0x80070005,
            /// <summary>
            /// The function encountered an invalid handle,
            /// probably due to a low-memory situation. 
            /// </summary>
            E_HANDLE = 0x80070006,
            /// <summary>
            /// The function received an invalid parameter.
            /// </summary>
            E_INVALIDARG = 0x80070057,
            /// <summary>
            /// Out of memory
            /// </summary>
            E_OUTOFMEMORY = 0x8007000E,
            /// <summary>
            /// Not implemented
            /// </summary>
            E_NOTIMPL = 0x80004001,
            /// <summary>
            /// Unknown error
            /// </summary>
            E_FAIL = 0x80000008,
            /// <summary>
            /// File not filtered due to password protection
            /// </summary>
            FILTER_E_PASSWORD = 0x8004170B,
            /// <summary>
            /// The document format is not recognised by the filter
            /// </summary>
            FILTER_E_UNKNOWNFORMAT = 0x8004170C,
            /// <summary>
            /// No text in current chunk
            /// </summary>
            FILTER_E_NO_TEXT = 0x80041705,
            /// <summary>
            /// No values in current chunk
            /// </summary>
            FILTER_E_NO_VALUES = 0x80041706,
            /// <summary>
            /// No more chunks of text available in object
            /// </summary>
            FILTER_E_END_OF_CHUNKS = 0x80041700,
            /// <summary>
            /// No more text available in chunk
            /// </summary>
            FILTER_E_NO_MORE_TEXT = 0x80041701,
            /// <summary>
            /// No more property values available in chunk
            /// </summary>
            FILTER_E_NO_MORE_VALUES = 0x80041702,
            /// <summary>
            /// Unable to access object
            /// </summary>
            FILTER_E_ACCESS = 0x80041703,
            /// <summary>
            /// Moniker doesn't cover entire region
            /// </summary>
            FILTER_W_MONIKER_CLIPPED = 0x00041704,
            /// <summary>
            /// Unable to bind IFilter for embedded object
            /// </summary>
            FILTER_E_EMBEDDING_UNAVAILABLE = 0x80041707,
            /// <summary>
            /// Unable to bind IFilter for linked object
            /// </summary>
            FILTER_E_LINK_UNAVAILABLE = 0x80041708,
            /// <summary>
            ///  This is the last text in the current chunk
            /// </summary>
            FILTER_S_LAST_TEXT = 0x00041709,
            /// <summary>
            /// This is the last value in the current chunk
            /// </summary>
            FILTER_S_LAST_VALUES = 0x0004170A
        }


        /// <summary>
        /// Flags controlling the operation of the FileFilter
        /// instance.
        /// </summary>
        [Flags]
        public enum IFILTER_INIT
        {
            /// <summary>
            /// Paragraph breaks should be marked with the Unicode PARAGRAPH
            /// SEPARATOR (0x2029)
            /// </summary>
            IFILTER_INIT_CANON_PARAGRAPHS = 1,

            /// <summary>
            /// Soft returns, such as the newline character in Microsoft Word, should
            /// be replaced by hard returnsLINE SEPARATOR (0x2028). Existing hard
            /// returns can be doubled. A carriage return (0x000D), line feed (0x000A),
            /// or the carriage return and line feed in combination should be considered
            /// a hard return. The intent is to enable pattern-expression matches that
            /// match against observed line breaks. 
            /// </summary>
            IFILTER_INIT_HARD_LINE_BREAKS = 2,

            /// <summary>
            /// Various word-processing programs have forms of hyphens that are not
            /// represented in the host character set, such as optional hyphens
            /// (appearing only at the end of a line) and nonbreaking hyphens. This flag
            /// indicates that optional hyphens are to be converted to nulls, and
            /// non-breaking hyphens are to be converted to normal hyphens (0x2010), or
            /// HYPHEN-MINUSES (0x002D). 
            /// </summary>
            IFILTER_INIT_CANON_HYPHENS = 4,

            /// <summary>
            /// Just as the IFILTER_INIT_CANON_HYPHENS flag standardizes hyphens,
            /// this one standardizes spaces. All special space characters, such as
            /// nonbreaking spaces, are converted to the standard space character
            /// (0x0020). 
            /// </summary>
            IFILTER_INIT_CANON_SPACES = 8,

            /// <summary>
            /// Indicates that the client wants text split into chunks representing
            /// public value-type properties. 
            /// </summary>
            IFILTER_INIT_APPLY_INDEX_ATTRIBUTES = 16,

            /// <summary>
            /// Indicates that the client wants text split into chunks representing
            /// properties determined during the indexing process. 
            /// </summary>
            IFILTER_INIT_APPLY_CRAWL_ATTRIBUTES = 256,

            /// <summary>
            /// Any properties not covered by the IFILTER_INIT_APPLY_INDEX_ATTRIBUTES
            /// and IFILTER_INIT_APPLY_CRAWL_ATTRIBUTES flags should be emitted. 
            /// </summary>
            IFILTER_INIT_APPLY_OTHER_ATTRIBUTES = 32,

            /// <summary>
            /// Optimizes IFilter for indexing because the client calls the
            /// IFilter::Init method only once and does not call IFilter::BindRegion.
            /// This eliminates the possibility of accessing a chunk both before and
            /// after accessing another chunk. 
            /// </summary>
            IFILTER_INIT_INDEXING_ONLY = 64,

            /// <summary>
            /// The text extraction process must recursively search all linked
            /// objects within the document. If a link is unavailable, the
            /// IFilter::GetChunk call that would have obtained the first chunk of the
            /// link should return FILTER_E_LINK_UNAVAILABLE. 
            /// </summary>
            IFILTER_INIT_SEARCH_LINKS = 128,

            /// <summary>
            /// The content indexing process can return property values set by the  filter. 
            /// </summary>
            IFILTER_INIT_FILTER_OWNED_VALUE_OK = 512
        }



        [Flags]
        public enum IFILTER_FLAGS
        {
            /// <summary>
            /// The caller should use the IPropertySetStorage and IPropertyStorage
            /// interfaces to locate additional properties. 
            /// When this flag is set, properties available through COM
            /// enumerators should not be returned from IFilter. 
            /// </summary>
            IFILTER_FLAGS_OLE_PROPERTIES = 1
        }


        public struct STAT_CHUNK
        {
            /// <summary>
            /// The chunk identifier. Chunk identifiers must be unique for the
            /// current instance of the IFilter interface. 
            /// Chunk identifiers must be in ascending order. The order in which
            /// chunks are numbered should correspond to the order in which they appear
            /// in the source document. Some search engines can take advantage of the
            /// proximity of chunks of various properties. If so, the order in which
            /// chunks with different properties are emitted will be important to the
            /// search engine. 
            /// </summary>
            public int idChunk;

            /// <summary>
            /// The type of break that separates the previous chunk from the current
            ///  chunk. Values are from the CHUNK_BREAKTYPE enumeration. 
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public CHUNK_BREAKTYPE breakType;

            /// <summary>
            /// Flags indicate whether this chunk contains a text-type or a
            /// value-type property. 
            /// Flag values are taken from the CHUNKSTATE enumeration. If the CHUNK_TEXT flag is set, 
            /// IFilter::GetText should be used to retrieve the contents of the chunk
            /// as a series of words. 
            /// If the CHUNK_VALUE flag is set, IFilter::GetValue should be used to retrieve 
            /// the value and treat it as a single property value. If the filter dictates that the same 
            /// content be treated as both text and as a value, the chunk should be emitted twice in two       
            /// different chunks, each with one flag set. 
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public CHUNKSTATE flags;

            /// <summary>
            /// The language and sublanguage associated with a chunk of text. Chunk locale is used 
            /// by document indexers to perform proper word breaking of text. If the chunk is 
            /// neither text-type nor a value-type with data type VT_LPWSTR, VT_LPSTR or VT_BSTR, 
            /// this field is ignored. 
            /// </summary>
            public int locale;

            /// <summary>
            /// The property to be applied to the chunk. If a filter requires that       the same text 
            /// have more than one property, it needs to emit the text once for each       property 
            /// in separate chunks. 
            /// </summary>
            public FULLPROPSPEC attribute;

            /// <summary>
            /// The ID of the source of a chunk. The value of the idChunkSource     member depends on the nature of the chunk: 
            /// If the chunk is a text-type property, the value of the idChunkSource       member must be the same as the value of the idChunk member. 
            /// If the chunk is an public value-type property derived from textual       content, the value of the idChunkSource member is the chunk ID for the
            /// text-type chunk from which it is derived. 
            /// If the filter attributes specify to return only public value-type
            /// properties, there is no content chunk from which to derive the current
            /// public value-type property. In this case, the value of the
            /// idChunkSource member must be set to zero, which is an invalid chunk. 
            /// </summary>
            public int idChunkSource;

            /// <summary>
            /// The offset from which the source text for a derived chunk starts in
            /// the source chunk. 
            /// </summary>
            public int cwcStartSource;

            /// <summary>
            /// The length in characters of the source text from which the current
            /// chunk was derived. 
            /// A zero value signifies character-by-character correspondence between
            /// the source text and 
            /// the derived text. A nonzero value means that no such direct
            /// correspondence exists
            /// </summary>
            public int cwcLenSource;
        }


        public enum CHUNKSTATE
        {
            /// <summary>
            /// The current chunk is a text-type property.
            /// </summary>
            CHUNK_TEXT = 0x1,
            /// <summary>
            /// The current chunk is a value-type property. 
            /// </summary>
            CHUNK_VALUE = 0x2,
            /// <summary>
            /// Reserved
            /// </summary>
            CHUNK_FILTER_OWNED_VALUE = 0x4
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PROPSPEC
        {
            [FieldOffset(0)]
            public int ulKind;     // 0 - string used; 1 - PROPID
            [FieldOffset(4)]
            public int propid;
            [FieldOffset(4)]
            public IntPtr lpwstr;
        }

        public struct FULLPROPSPEC
        {
            public Guid guidPropSet;
            public PROPSPEC psProperty;
        }

        /// <summary>
        /// Enumerates the different breaking types that occur between 
        /// chunks of text read out by the FileFilter.
        /// </summary>
        public enum CHUNK_BREAKTYPE
        {
            /// <summary>
            /// No break is placed between the current chunk and the previous chunk.
            /// The chunks are glued together. 
            /// </summary>
            CHUNK_NO_BREAK = 0,
            /// <summary>
            /// A word break is placed between this chunk and the previous chunk that
            /// had the same attribute. 
            /// Use of CHUNK_EOW should be minimized because the choice of word
            /// breaks is language-dependent, 
            /// so determining word breaks is best left to the search engine. 
            /// </summary>
            CHUNK_EOW = 1,
            /// <summary>
            /// A sentence break is placed between this chunk and the previous chunk
            /// that had the same attribute. 
            /// </summary>
            CHUNK_EOS = 2,
            /// <summary>
            /// A paragraph break is placed between this chunk and the previous chunk
            /// that had the same attribute.
            /// </summary>     
            CHUNK_EOP = 3,
            /// <summary>
            /// A chapter break is placed between this chunk and the previous chunk
            /// that had the same attribute. 
            /// </summary>
            CHUNK_EOC = 4
        }

        static void CleanUpCharacters(int chBuf, StringBuilder buf)
        {
            // The game here is to fold any "cute" versions of characters to thier 
            // simplified form to make parsing easier.

            // Truncate any extra chars that may have been writting to the buffer.
            buf.Remove(chBuf, buf.Length - chBuf);

            for (int i = 0; i < chBuf; i++)
            {
                char ch = buf[i];
                int chi = ch;
                switch (chi)
                {
                    case 0:        // embedded null
                    case 0x2000:   // en quad
                    case 0x2001:   // em quad
                    case 0x2002:   // en space
                    case 0x2003:   // em space
                    case 0x2004:   // three-per-em space
                    case 0x2005:   // four-per-em space
                    case 0x2006:   // six-per-em space
                    case 0x2007:   // figure space
                    case 0x2008:   // puctuation space
                    case 0x2009:   // thin space
                    case 0x200A:   // hair space
                    case 0x200B:   // zero-width space
                    case 0x200C:   // zero-width non-joiner
                    case 0x200D:   // zero-width joiner
                    case 0x202f:   // no-break space
                    case 0x3000:   // ideographic space
                        buf[i] = ' ';
                        break;

                    case 0x00B6:   // pilcro
                    case 0x2028:   // line seperator
                    case 0x2029:   // paragraph seperator
                        buf[i] = '\n';
                        break;

                    case 0x00AD:   // soft-hyphen
                    case 0x00B7:   // middle dot
                    case 0x2010:   // hyphen
                    case 0x2011:   // non-breaking hyphen
                    case 0x2012:   // figure dash
                    case 0x2013:   // en dash
                    case 0x2014:   // em dash
                    case 0x2015:   // quote dash
                    case 0x2027:   // hyphenation point
                    case 0x2043:   // hyphen bullet
                    case 0x208B:   // subscript minus
                    case 0xFE31:   // vertical em dash
                    case 0xFE32:   // vertical en dash
                    case 0xFE58:   // small em dash
                    case 0xFE63:   // small hyphen minus
                        buf[i] = '-';
                        break;

                    case 0x00B0:   // degree
                    case 0x2018:   // left single quote
                    case 0x2019:   // right single quote
                    case 0x201A:   // low right single quote
                    case 0x201B:   // high left single quote
                    case 0x2032:   // prime
                    case 0x2035:   // reversed prime
                    case 0x2039:   // left-pointing angle quotation mark
                    case 0x203A:   // right-pointing angle quotation mark
                        buf[i] = '\'';
                        break;

                    case 0x201C:   // left double quote
                    case 0x201D:   // right double quote
                    case 0x201E:   // low right double quote
                    case 0x201F:   // high left double quote
                    case 0x2033:   // double prime
                    case 0x2034:   // triple prime
                    case 0x2036:   // reversed double prime
                    case 0x2037:   // reversed triple prime
                    case 0x00AB:   // left-pointing double angle quotation mark
                    case 0x00BB:   // right-pointing double angle quotation mark
                    case 0x3003:   // ditto mark
                    case 0x301D:   // reversed double prime quotation mark
                    case 0x301E:   // double prime quotation mark
                    case 0x301F:   // low double prime quotation mark
                        buf[i] = '\"';
                        break;

                    case 0x00A7:   // section-sign
                    case 0x2020:   // dagger
                    case 0x2021:   // double-dagger
                    case 0x2022:   // bullet
                    case 0x2023:   // triangle bullet
                    case 0x203B:   // reference mark
                    case 0xFE55:   // small colon
                        buf[i] = ':';
                        break;

                    case 0x2024:   // one dot leader
                    case 0x2025:   // two dot leader
                    case 0x2026:   // elipsis
                    case 0x3002:   // ideographic full stop
                    case 0xFE30:   // two dot vertical leader
                    case 0xFE52:   // small full stop
                        buf[i] = '.';
                        break;

                    case 0x3001:   // ideographic comma
                    case 0xFE50:   // small comma
                    case 0xFE51:   // small ideographic comma
                        buf[i] = ',';
                        break;

                    case 0xFE54:   // small semicolon
                        buf[i] = ';';
                        break;

                    case 0x00A6:   // broken-bar
                    case 0x2016:   // double vertical line
                        buf[i] = '|';
                        break;

                    case 0x2017:   // double low line
                    case 0x203E:   // overline
                    case 0x203F:   // undertie
                    case 0x2040:   // character tie
                    case 0xFE33:   // vertical low line
                    case 0xFE49:   // dashed overline
                    case 0xFE4A:   // centerline overline
                    case 0xFE4D:   // dashed low line
                    case 0xFE4E:   // centerline low line
                        buf[i] = '_';
                        break;

                    case 0x301C:   // wave dash
                    case 0x3030:   // wavy dash
                    case 0xFE34:   // vertical wavy low line
                    case 0xFE4B:   // wavy overline
                    case 0xFE4C:   // double wavy overline
                    case 0xFE4F:   // wavy low line
                        buf[i] = '~';
                        break;

                    case 0x2038:   // caret
                    case 0x2041:   // caret insertion point
                        buf[i] = '^';
                        break;

                    case 0x2030:   // per-mille
                    case 0x2031:   // per-ten thousand
                    case 0xFE6A:   // small per-cent
                        buf[i] = '%';
                        break;

                    case 0xFE6B:   // small commercial at
                        buf[i] = '@';
                        break;

                    case 0x00A9:   // copyright
                        buf[i] = 'c';
                        break;

                    case 0x00B5:   // micro
                        buf[i] = 'u';
                        break;

                    case 0x00AE:   // registered
                        buf[i] = 'r';
                        break;

                    case 0x207A:   // superscript plus
                    case 0x208A:   // subscript plus
                    case 0xFE62:   // small plus
                        buf[i] = '+';
                        break;

                    case 0x2044:   // fraction slash
                        buf[i] = '/';
                        break;

                    case 0x2042:   // asterism
                    case 0xFE61:   // small asterisk
                        buf[i] = '*';
                        break;

                    case 0x208C:   // subscript equal
                    case 0xFE66:   // small equal
                        buf[i] = '=';
                        break;

                    case 0xFE68:   // small reverse solidus
                        buf[i] = '\\';
                        break;

                    case 0xFE5F:   // small number sign
                        buf[i] = '#';
                        break;

                    case 0xFE60:   // small ampersand
                        buf[i] = '&';
                        break;

                    case 0xFE69:   // small dollar sign
                        buf[i] = '$';
                        break;

                    case 0x2045:   // left square bracket with quill
                    case 0x3010:   // left black lenticular bracket
                    case 0x3016:   // left white lenticular bracket
                    case 0x301A:   // left white square bracket
                    case 0xFE3B:   // vertical left lenticular bracket
                    case 0xFF41:   // vertical left corner bracket
                    case 0xFF43:   // vertical white left corner bracket
                        buf[i] = '[';
                        break;

                    case 0x2046:   // right square bracket with quill
                    case 0x3011:   // right black lenticular bracket
                    case 0x3017:   // right white lenticular bracket
                    case 0x301B:   // right white square bracket
                    case 0xFE3C:   // vertical right lenticular bracket
                    case 0xFF42:   // vertical right corner bracket
                    case 0xFF44:   // vertical white right corner bracket
                        buf[i] = ']';
                        break;

                    case 0x208D:   // subscript left parenthesis
                    case 0x3014:   // left tortise-shell bracket
                    case 0x3018:   // left white tortise-shell bracket
                    case 0xFE35:   // vertical left parenthesis
                    case 0xFE39:   // vertical left tortise-shell bracket
                    case 0xFE59:   // small left parenthesis
                    case 0xFE5D:   // small left tortise-shell bracket
                        buf[i] = '(';
                        break;

                    case 0x208E:   // subscript right parenthesis
                    case 0x3015:   // right tortise-shell bracket
                    case 0x3019:   // right white tortise-shell bracket
                    case 0xFE36:   // vertical right parenthesis
                    case 0xFE3A:   // vertical right tortise-shell bracket
                    case 0xFE5A:   // small right parenthesis
                    case 0xFE5E:   // small right tortise-shell bracket
                        buf[i] = ')';
                        break;

                    case 0x3008:   // left angle bracket
                    case 0x300A:   // left double angle bracket
                    case 0xFF3D:   // vertical left double angle bracket
                    case 0xFF3F:   // vertical left angle bracket
                    case 0xFF64:   // small less-than
                        buf[i] = '<';
                        break;

                    case 0x3009:   // right angle bracket
                    case 0x300B:   // right double angle bracket
                    case 0xFF3E:   // vertical right double angle bracket
                    case 0xFF40:   // vertical right angle bracket
                    case 0xFF65:   // small greater-than
                        buf[i] = '>';
                        break;

                    case 0xFE37:   // vertical left curly bracket
                    case 0xFE5B:   // small left curly bracket
                        buf[i] = '{';
                        break;

                    case 0xFE38:   // vertical right curly bracket
                    case 0xFE5C:   // small right curly bracket
                        buf[i] = '}';
                        break;

                    case 0x00A1:   // inverted exclamation mark
                    case 0x00AC:   // not
                    case 0x203C:   // double exclamation mark
                    case 0x203D:   // interrobang
                    case 0xFE57:   // small exclamation mark
                        buf[i] = '!';
                        break;

                    case 0x00BF:   // inverted question mark
                    case 0xFE56:   // small question mark
                        buf[i] = '?';
                        break;

                    case 0x00B9:   // superscript one
                        buf[i] = '1';
                        break;

                    case 0x00B2:   // superscript two
                        buf[i] = '2';
                        break;

                    case 0x00B3:   // superscript three
                        buf[i] = '3';
                        break;

                    case 0x2070:   // superscript zero
                    case 0x2074:   // superscript four
                    case 0x2075:   // superscript five
                    case 0x2076:   // superscript six
                    case 0x2077:   // superscript seven
                    case 0x2078:   // superscript eight
                    case 0x2079:   // superscript nine
                    case 0x2080:   // subscript zero
                    case 0x2081:   // subscript one
                    case 0x2082:   // subscript two
                    case 0x2083:   // subscript three
                    case 0x2084:   // subscript four
                    case 0x2085:   // subscript five
                    case 0x2086:   // subscript six
                    case 0x2087:   // subscript seven
                    case 0x2088:   // subscript eight
                    case 0x2089:   // subscript nine
                    case 0x3021:   // Hangzhou numeral one
                    case 0x3022:   // Hangzhou numeral two
                    case 0x3023:   // Hangzhou numeral three
                    case 0x3024:   // Hangzhou numeral four
                    case 0x3025:   // Hangzhou numeral five
                    case 0x3026:   // Hangzhou numeral six
                    case 0x3027:   // Hangzhou numeral seven
                    case 0x3028:   // Hangzhou numeral eight
                    case 0x3029:   // Hangzhou numeral nine
                        chi = chi & 0x000F;
                        buf[i] = System.Convert.ToChar(chi);
                        break;

                    // ONE is at ZERO location... careful
                    case 0x3220:   // parenthesized ideograph one
                    case 0x3221:   // parenthesized ideograph two
                    case 0x3222:   // parenthesized ideograph three
                    case 0x3223:   // parenthesized ideograph four
                    case 0x3224:   // parenthesized ideograph five
                    case 0x3225:   // parenthesized ideograph six
                    case 0x3226:   // parenthesized ideograph seven
                    case 0x3227:   // parenthesized ideograph eight
                    case 0x3228:   // parenthesized ideograph nine
                    case 0x3280:   // circled ideograph one
                    case 0x3281:   // circled ideograph two
                    case 0x3282:   // circled ideograph three
                    case 0x3283:   // circled ideograph four
                    case 0x3284:   // circled ideograph five
                    case 0x3285:   // circled ideograph six
                    case 0x3286:   // circled ideograph seven
                    case 0x3287:   // circled ideograph eight
                    case 0x3288:   // circled ideograph nine
                        chi = (chi & 0x000F) + 1;
                        buf[i] = System.Convert.ToChar(chi);
                        break;

                    case 0x3007:   // ideographic number zero
                    case 0x24EA:   // circled number zero
                        buf[i] = '0';
                        break;

                    default:
                        if (0xFF01 <= ch           // fullwidth exclamation mark 
                            && ch <= 0xFF5E)       // fullwidth tilde
                        {
                            // the fullwidths line up with ASCII low subset
                            buf[i] = System.Convert.ToChar(chi & 0xFF00 + '!' - 1);
                            //ch = ch & 0xFF00 + '!' - 1;               
                        }
                        else if (0x2460 <= ch      // circled one
                                 && ch <= 0x2468)  // circled nine
                        {
                            buf[i] = System.Convert.ToChar(chi - 0x2460 + '1');
                            //ch = ch - 0x2460 + '1';
                        }
                        else if (0x2474 <= ch      // parenthesized one
                                 && ch <= 0x247C)  // parenthesized nine
                        {
                            buf[i] = System.Convert.ToChar(chi - 0x2474 + '1');
                            // ch = ch - 0x2474 + '1';
                        }
                        else if (0x2488 <= ch      // one full stop
                                 && ch <= 0x2490)  // nine full stop
                        {
                            buf[i] = System.Convert.ToChar(chi - 0x2488 + '1');
                            //ch = ch - 0x2488 + '1';
                        }
                        else if (0x249C <= ch      // parenthesized small a
                                 && ch <= 0x24B5)  // parenthesized small z
                        {
                            buf[i] = System.Convert.ToChar(chi - 0x249C + 'a');
                            //ch = ch - 0x249C + 'a';
                        }
                        else if (0x24B6 <= ch      // circled capital A
                                 && ch <= 0x24CF)  // circled capital Z
                        {
                            buf[i] = System.Convert.ToChar(chi - 0x24B6 + 'A');
                            //ch = ch - 0x24B6 + 'A';
                        }
                        else if (0x24D0 <= ch      // circled small a
                                 && ch <= 0x24E9)  // circled small z
                        {
                            buf[i] = System.Convert.ToChar(chi - 0x24D0 + 'a');
                            //ch = ch - 0x24D0 + 'a';
                        }
                        else if (0x2500 <= ch      // box drawing (begin)
                                 && ch <= 0x257F)  // box drawing (end)
                        {
                            buf[i] = '|';
                        }
                        else if (0x2580 <= ch      // block elements (begin)
                                 && ch <= 0x259F)  // block elements (end)
                        {
                            buf[i] = '#';
                        }
                        else if (0x25A0 <= ch      // geometric shapes (begin)
                                 && ch <= 0x25FF)  // geometric shapes (end)
                        {
                            buf[i] = '*';
                        }
                        else if (0x2600 <= ch      // dingbats (begin)
                                 && ch <= 0x267F)  // dingbats (end)
                        {
                            buf[i] = '.';
                        }
                        //else
                        //   ValidUnicode(ch);   // validate that it's legit Unicode
                        break;
                }
            }
        }
    

    }
}
