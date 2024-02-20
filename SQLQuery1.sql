USE [PWS]
GO

DECLARE	@return_value Int

EXEC	@return_value = [DD].[usp_EntityID_Match]
		@UserID = 201,
		@Match = N'liq',
		@Top = 20

SELECT	@return_value as 'Return Value'

GO
