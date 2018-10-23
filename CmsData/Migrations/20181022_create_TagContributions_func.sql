﻿CREATE proc [dbo].[TagContributions] (@tagname varchar(50), @json varchar(max))
as
begin
	declare @AddToTag int = (select json_value(@json, '$.AddToTag'))
	declare @Priority int = (select json_value(@json, '$.Priority'))

	if isnull(@AddToTag, 0) = 0
	begin
		delete dbo.ContributionTag where TagName = @tagname

		insert dbo.ContributionTag ( ContributionId , TagName, [Priority] )
		select cs.ContributionId, @tagname, @priority
		from dbo.Contributions2SearchIds(@json) cs
	end
	else
	begin
		insert dbo.ContributionTag ( ContributionId , TagName, [Priority] )
		select cs.ContributionId, @tagname, @Priority
		from dbo.Contributions2SearchIds(@json) cs
		where not exists(select null from dbo.ContributionTag ct where cs.ContributionId = ct.ContributionId and ct.TagName = @tagname)
	end

end
