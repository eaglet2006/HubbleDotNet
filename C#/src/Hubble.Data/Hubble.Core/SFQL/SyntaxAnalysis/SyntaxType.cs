/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public enum SyntaxType
    {
        Eof   = 0,
        Space = 1,
        Comment = 2,
        Numeric = 3,
        String  = 4,

        //Logic Operators
        OR  = 5, //OR
        AND = 6, //And
        NOT = 7, //Not

        //Comparison Operators
        NotEqual      = 8, // <>
        Equal         = 9, // =
        Lessthan      = 10, // <
        LessthanEqual = 11, // <=
        Largethan     = 12, // >
        LargethanEqual= 13, // >=

        //Arithmetic Operators
        Plus     = 14, //+
        Subtract = 15, //-
        Multiply = 16, //*
        Divide   = 17, // /
        Mod      = 18, // %

        //bracket
        LBracket = 20, //(
        RBracket = 21, //)
        LSquareBracket = 22, //[
        RSquareBracket = 23, //]

        //Symbol
        Up         = 26, //^
        Comma      = 27, //,
        Semicolon  = 28, //;
        Dot        = 29, //.
        Colon      = 30, //:
        At         = 31, //@

        // Identifer
        Identifer = 35, // Identifer

        //Keywords
        BEGIN_KEYWORD = 36,
        ALTER     , 
        ANALYZER  , 
        AS        ,
        ASC       ,
        AVG       ,
        BEGIN     ,
        BETWEEN   ,
        BY        ,
        CASE      ,
        CHECK     ,
        CLUSTERED ,
        COALESCE  ,
        COMMIT    ,
        COMMITTED ,

        //for user query interface
        CONTAINS  ,
        CONTAINS1 ,
        CONTAINS2 ,
        CONTAINS3 ,

        COUNT     ,
        CREATE    ,
        DATABASE  ,
        DEFAULT   ,
        DELETE    ,
        DESC      ,
        DISK      ,
        DISTINCT  ,
        DISTRIBUTED,
        DROP      ,
        EXEC      ,
        EXISTS    ,
        EXIT      ,
        FILE      ,
        FOR      ,
        FOREIGN      ,
        FROM      ,
        GROUP      ,
        IDENTITY      ,
        IN      ,
        INDEX      ,
        INNER      ,
        INSERT      ,
        INTO      ,
        IS      ,
        JOIN      ,
        KEY      ,
        LEFT      ,
        LIKE     ,
        MATCH    ,
        MAX      ,
        MIN      ,
        NOCHECK      ,
        NONCLUSTERED ,
        NONE     ,
        NULL      ,
        OF      ,
        ON      ,
        ORDER      ,
        OUTER      ,
        PRIMARY      ,
        RIGHT      ,
        ROLLBACK      ,
        SELECT      ,
        SET,
        SUM,
        TABLE      ,
        TO      ,
        TOKENIZED,
        TOP      ,
        TRAN      ,
        TRUNCATE      ,
        UNCOMMITTED      ,
        UNION      ,
        UNIQUE      ,
        UNTOKENIZED,
        UPDATE      ,
        USE      ,
        VALUES      ,
        VIEW      ,
        WHERE      ,
        END_KEYWORD,

    }
}
