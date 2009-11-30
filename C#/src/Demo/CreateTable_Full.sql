exec sp_droptable 'news';


[Directory('d:\test\news\')]

[DBAdapter('SQLSERVER2005')]

[DBConnect('Data Source=(local);Initial Catalog=News;Integrated Security=True')]

Create table News

(

Title nvarchar(max) Tokenized Analyzer 'PanGuSegment' NOT NULL Default '',

Content nvarchar(max) Tokenized Analyzer 'PanGuSegment' NOT NULL Default '',

Time Date Untokenized NOT NULL Default '1990-01-01',

Url    nvarchar(max) NULL

);