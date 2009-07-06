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
        MAX      ,
        MIN      ,
        NOCHECK      ,
        NONCLUSTERED      ,
        NULL      ,
        OF      ,
        ON      ,
        ORDER      ,
        OUTER      ,
        PRIMARY      ,
        RIGHT      ,
        ROLLBACK      ,
        SELECT      ,
        SUM      ,
        TABLE      ,
        TO      ,
        TOP      ,
        TRAN      ,
        TRUNCATE      ,
        UNCOMMITTED      ,
        UNION      ,
        UNIQUE      ,
        UPDATE      ,
        USE      ,
        VALUES      ,
        VIEW      ,
        WHERE      ,
        END_KEYWORD,

    }
}
