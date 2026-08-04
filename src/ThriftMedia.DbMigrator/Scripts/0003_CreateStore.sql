-- Thrift store (one store per app user)
CREATE TABLE IF NOT EXISTS retail."Store"
(
    "Id"               SERIAL          PRIMARY KEY,
    "StoreName"        VARCHAR(100)    NOT NULL,
    "PhoneNumber"      VARCHAR(50)     NULL,
    "WebsiteUrl"       VARCHAR(255)    NULL,
    "IsActive"         BOOLEAN         NOT NULL DEFAULT FALSE,
    "IsSuspended"      BOOLEAN         NOT NULL DEFAULT FALSE,

    -- Owner contact
    "OwnerFirstName"   VARCHAR(50)     NULL,
    "OwnerLastName"    VARCHAR(50)     NULL,
    "OwnerPhoneNumber" VARCHAR(50)     NULL,
    "OwnerEmail"       VARCHAR(255)    NULL,

    -- Business licence
    "LicenseNumber"    VARCHAR(100)    NOT NULL,
    "LicenseType"      VARCHAR(50)     NOT NULL,
    "IssueingAuthority" VARCHAR(100)   NOT NULL,
    "IssueDate"        TIMESTAMP       NOT NULL,
    "ExpirationDate"   TIMESTAMP       NULL,
    "LicenseStatus"    VARCHAR(20)     NOT NULL,

    -- Address
    "Address1"         VARCHAR(150)    NOT NULL,
    "Address2"         VARCHAR(150)    NOT NULL DEFAULT '',
    "City"             VARCHAR(100)    NOT NULL,
    "ProvinceState"    VARCHAR(50)     NULL,
    "PostalCode"       VARCHAR(20)     NOT NULL,
    "Country"          VARCHAR(100)    NULL,

    -- Audit
    "CreatedBy"        VARCHAR(100)    NOT NULL,
    "CreatedAt"        TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"        VARCHAR(100)    NULL,
    "UpdatedAt"        TIMESTAMP       NULL,

    -- FK to owning app user
    "AppUserId"        INTEGER         NOT NULL,

    CONSTRAINT "FK_Store_AppUser" FOREIGN KEY ("AppUserId")
        REFERENCES auth."AppUsers" ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "UQ_Store_AppUserId"
    ON retail."Store" ("AppUserId");
