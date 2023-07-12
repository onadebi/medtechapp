select es.* from "EmploymentStatus" es ;
select gc.* from "GenderCategory" gc ;
select cd.* from "CountryDetail" cd ;
select sd.* from "StateDetail" sd ;

--==Company details
select mcd.* from "MedicCompanyDetail" mcd ;
select bd.* from "BranchDetail" bd;

select it.* from "IdentificationType" it ;

select mc.* from "MenuController" mc ;
select mca.* from "MenuControllerActions" mca ;

select mb."IsUsed",mb."IsProcessed" , mb."UpdatedAt", mb.* from "MessageBox" mb ;

select umcap.* from "UserGroupMenuControllerActionPermissions" umcap ;
select up.* from "UserProfile" up ;
select upg.* from "UserProfileGroup" upg ;
select ug.* from "UserGroup" ug ;

select aal."CreatedAt", aal.* from "AppActivityLog" aal order by 1 desc ;
--====
truncate table "MenuControllerActions" cascade;
truncate table "MenuController" cascade;

delete from "UserProfileGroup";
delete  from "UserProfile";
delete from "UserGroup";
delete from "MedicCompanyDetail";
delete from "BranchDetail";