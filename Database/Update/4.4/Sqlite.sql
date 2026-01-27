CREATE TABLE [AuditTrail] (
  [ID] INTEGER NOT NULL
, [EntityType] nvarchar(100) NOT NULL
, [EntityID] nchar(50) NOT NULL
, [IPAddress] nchar(10) NULL
, [Changes] ntext NULL
, [Title] nvarchar(200) NULL
, [Description] nvarchar(500) NULL
, [Status] int NULL
, [CreateBy] nvarchar(50) NULL
, [CreatebyName] nvarchar(100) NULL
, [CreateDate] datetime NULL
, [LastUpdateBy] nvarchar(50) NULL
, [LastUpdateByName] nvarchar(100) NULL
, [LastUpdateDate] datetime NULL
, CONSTRAINT [PK_AuditTrail] PRIMARY KEY ([ID])
);