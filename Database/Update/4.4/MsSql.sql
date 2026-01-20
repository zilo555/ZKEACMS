IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AuditTrail' AND xtype='U')
BEGIN
	CREATE TABLE [dbo].[AuditTrail](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[EntityType] [nvarchar](100) NOT NULL,
		[EntityID] [nchar](50) NOT NULL,
		[IPAddress] [nchar](10) NULL,
		[Changes] [nvarchar](max) NULL,
		[Title] [nvarchar](200) NULL,
		[Description] [nvarchar](500) NULL,
		[Status] [int] NULL,
		[CreateBy] [nvarchar](50) NULL,
		[CreatebyName] [nvarchar](100) NULL,
		[CreateDate] [datetime2](7) NULL,
		[LastUpdateBy] [nvarchar](50) NULL,
		[LastUpdateByName] [nvarchar](100) NULL,
		[LastUpdateDate] [datetime2](7) NULL,
	CONSTRAINT [PK_AuditTrail] PRIMARY KEY CLUSTERED
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END