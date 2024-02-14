namespace MapShared.Dto;

public record CreateOrganizationDto(
    string OrgName,
    string OrgArea,
    ContactsDto Contacts,
    OwnerInfoDto Owner
    );

public record OwnerInfoDto(
    string Fullname,
    string Email,
    string Password
);


public record ContactsDto(
    string Site,
    string Phone,
    string Address
);
