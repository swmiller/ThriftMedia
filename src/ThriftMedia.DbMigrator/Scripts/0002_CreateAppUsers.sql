-- Application users (OAuth/OIDC identities)
CREATE TABLE IF NOT EXISTS auth."AppUsers"
(
    "Id"            SERIAL          PRIMARY KEY,
    "Provider"      VARCHAR(200)    NOT NULL,
    "ProviderSub"   VARCHAR(200)    NOT NULL,
    "Email"         VARCHAR(320)    NULL,
    "DisplayName"   VARCHAR(200)    NULL,
    "Role"          INTEGER         NOT NULL DEFAULT 0,
    "CreatedAtUtc"  TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastSeenAtUtc" TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "UQ_AppUsers_Provider_ProviderSub" UNIQUE ("Provider", "ProviderSub")
);

-- Only one site admin is permitted
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AppUsers_SingleSiteAdmin"
    ON auth."AppUsers" ("Role")
    WHERE "Role" = 0;

CREATE INDEX IF NOT EXISTS "IX_AppUsers_Provider_ProviderSub"
    ON auth."AppUsers" ("Provider", "ProviderSub");
