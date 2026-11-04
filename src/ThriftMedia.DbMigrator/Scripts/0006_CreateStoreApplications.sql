-- Store applications submitted from the public "Apply" page and reviewed by the SystemAdmin.
-- Status: 0 = Pending, 1 = Approved, 2 = Rejected
-- Approval creates the AppUser (Role = StoreAdmin, Status = Approved) and the Store row.
CREATE TABLE IF NOT EXISTS retail."StoreApplication"
(
    "Id"                    SERIAL          PRIMARY KEY,
    "Status"                INTEGER         NOT NULL DEFAULT 0,

    -- Applicant and store details
    "StoreName"             VARCHAR(100)    NOT NULL,
    "OwnerFullName"         VARCHAR(100)    NOT NULL,
    "Email"                 VARCHAR(320)    NOT NULL,
    "PhoneNumber"           VARCHAR(50)     NOT NULL,
    "Address1"              VARCHAR(150)    NOT NULL,
    "Address2"              VARCHAR(150)    NOT NULL DEFAULT '',
    "City"                  VARCHAR(100)    NOT NULL,
    "ProvinceState"         VARCHAR(50)     NOT NULL,
    "PostalCode"            VARCHAR(20)     NOT NULL,
    "Country"               VARCHAR(100)    NOT NULL DEFAULT 'United States',
    "WebsiteUrl"            VARCHAR(255)    NULL,
    "Description"           VARCHAR(1000)   NOT NULL,

    -- Verification documents (references to stored files; storage is decided separately)
    "LicenseNumber"         VARCHAR(100)    NOT NULL,
    "LicenseImageRef"       VARCHAR(500)    NOT NULL,
    "StoreFrontImageRef"    VARCHAR(500)    NULL,

    -- Submission and review
    "SubmittedAtUtc"        TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ReviewedByAppUserId"   INTEGER         NULL,
    "ReviewedAtUtc"         TIMESTAMP       NULL,
    "ReviewNotes"           VARCHAR(1000)   NULL,

    -- Set when the application is approved
    "AppUserId"             INTEGER         NULL,

    CONSTRAINT "CK_StoreApplication_Status" CHECK ("Status" IN (0, 1, 2)),
    CONSTRAINT "FK_StoreApplication_ReviewedBy" FOREIGN KEY ("ReviewedByAppUserId")
        REFERENCES auth."AppUsers" ("Id"),
    CONSTRAINT "FK_StoreApplication_AppUser" FOREIGN KEY ("AppUserId")
        REFERENCES auth."AppUsers" ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_StoreApplication_Status_SubmittedAtUtc"
    ON retail."StoreApplication" ("Status", "SubmittedAtUtc");

CREATE INDEX IF NOT EXISTS "IX_StoreApplication_Email"
    ON retail."StoreApplication" (LOWER("Email"));
