IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[FindPersonByExtraValue]') AND type in (N'FN', N'IF',N'TF', N'FS', N'FT'))
BEGIN	
	DROP FUNCTION [dbo].[FindPersonByExtraValue]	
END
GO

CREATE FUNCTION [dbo].[FindPersonByExtraValue](@key nvarchar(100), @value nvarchar(max))
RETURNS @t TABLE ( PeopleId INT)
AS
BEGIN

	INSERT INTO @t
	SELECT PeopleId 
	FROM dbo.PeopleExtra
	WHERE Field = @Key
	AND ( StrValue = @Value
        OR DateValue = dbo.ParseDate( @Value)
        OR Data = @Value
        OR IntValue = @Value
        OR IntValue2 = @Value
        OR BitValue = @Value
        OR FieldValue = @Value )
	
	RETURN
END
GO
