-- Copyright (c) Microsoft Corporation.
-- Licensed under the MIT license.

DECLARE @current_version bigint
DECLARE @target_version bigint
DECLARE @upgrade_version bigint

SET @target_version = $(targetVersion)
--SET @target_version = 2

IF NOT EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'MetadataVersion' AND sch.name = 'dbo')
BEGIN
	CREATE TABLE [dbo].[MetadataVersion] (currentVersion bigint)
	INSERT INTO [dbo].[MetadataVersion] VALUES (1)
	SET @current_version = 1
END
ELSE BEGIN
	SELECT @current_version = currentVersion FROM [dbo].[MetadataVersion]
END

--- Upgrade from Version 1 to Version 2 ---
SET @upgrade_version = 2
IF (@current_version < @upgrade_version AND @target_version >= @upgrade_version)
BEGIN
	PRINT 'Upgrade from version 1 to version 2'
	CREATE TABLE [dbo].[TelemetryDataConnectors]( 
		[Id] [bigint] IDENTITY(1,1) NOT NULL, 
		[Name] [nvarchar](64) NOT NULL, 
		[Type] [nvarchar](512) NOT NULL, 
		[Configuration] [nvarchar](max) NOT NULL, 
		PRIMARY KEY (Id) 
	) ON [PRIMARY]

	IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'CustomMeterDimensions' AND sch.name = 'dbo')
	BEGIN
		DROP TABLE [dbo].[CustomMeterDimensions]
	END
	
	IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'CustomMeters' AND sch.name = 'dbo')
	BEGIN
		DROP TABLE [dbo].[CustomMeters]
	END
	
	
	CREATE TABLE [dbo].[CustomMeters](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[OfferId] [bigint] NOT NULL,
		[MeterName] [nvarchar](50) NOT NULL,
		[TelemetryDataConnectorId] [bigint] NOT NULL,
		[TelemetryQuery] [nvarchar](max) NOT NULL,
		PRIMARY KEY (id),
		CONSTRAINT FK_telemetry_data_connector_id_custom_meters FOREIGN KEY (TelemetryDataConnectorId)
		REFERENCES TelemetryDataConnectors(Id)
	) ON [PRIMARY]
	
	CREATE TABLE [dbo].[CustomMeterDimensions](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[MeterId] bigint NOT NULL,
		[PlanId] bigint NOT NULL,
		[MonthlyUnlimited] [bit] NULL,
		[AnnualUnlimited] [bit] NULL,
		[MonthlyQuantityIncludedInBase] [int] NULL,
		[AnnualQuantityIncludedInBase] [int] NULL,
		PRIMARY KEY (Id),
		CONSTRAINT FK_meter_id_custom_meter_dimensions FOREIGN KEY (MeterId)
		REFERENCES CustomMeters(Id),
		CONSTRAINT FK_plan_id_custom_meter_dimensions FOREIGN KEY (PlanId)
		REFERENCES Plans(Id)
	) ON [PRIMARY]
	
	CREATE TABLE [dbo].[SubscriptionCustomMeterUsages](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[MeterId] bigint NOT NULL,
		[SubscriptionId] uniqueidentifier NOT NULL,
		[CreatedTime] [datetime2] NOT NULL,
		[LastUpdatedTime] [datetime2],
		[LastErrorReportedTime] [datetime2],
		[LastError] [nvarchar](max),
		[IsEnabled] [bit],
		[UnsubscribedTime] [datetime2],
		[EnabledTime] [datetime2],
		[DisabledTime] [datetime2],
		PRIMARY KEY (Id),
		CONSTRAINT FK_meter_id_subscription_custom_meter_usage FOREIGN KEY (MeterId)
		REFERENCES CustomMeters(Id),
		CONSTRAINT FK_subscription_id_subscription_custom_meter_usage FOREIGN KEY (SubscriptionId)
		REFERENCES Subscriptions(SubscriptionId)
	) ON [PRIMARY]
	
END

UPDATE [dbo].[MetadataVersion] SET currentVersion = @target_version
