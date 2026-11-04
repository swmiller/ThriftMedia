-- Authorization data for federated sign-in (see docs/adr/0001-authentication-authorization-approach.md).
--
-- AppUsers.Role   : 0 = SystemAdmin (existing), 1 = StoreAdmin
-- AppUsers.Status : 0 = Approved (account exists, awaiting first sign-in), 1 = Active, 2 = Suspended
--
-- A store user is pre-created when the SystemAdmin approves a store application. The identity
-- provider's subject is not known until the first sign-in, which links the provider identity to
-- the pre-created row by verified email. Provider and ProviderSub therefore become nullable.

ALTER TABLE auth."AppUsers" ALTER COLUMN "Provider"    DROP NOT NULL;
ALTER TABLE auth."AppUsers" ALTER COLUMN "ProviderSub" DROP NOT NULL;

-- Existing rows (the System Admin, if any) are already signed in, so they start Active.
-- The default is dropped afterwards so new rows must state their status explicitly.
ALTER TABLE auth."AppUsers" ADD COLUMN IF NOT EXISTS "Status" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE auth."AppUsers" ALTER COLUMN "Status" DROP DEFAULT;

ALTER TABLE auth."AppUsers" DROP CONSTRAINT IF EXISTS "CK_AppUsers_Role";
ALTER TABLE auth."AppUsers" ADD CONSTRAINT "CK_AppUsers_Role" CHECK ("Role" IN (0, 1));

ALTER TABLE auth."AppUsers" DROP CONSTRAINT IF EXISTS "CK_AppUsers_Status";
ALTER TABLE auth."AppUsers" ADD CONSTRAINT "CK_AppUsers_Status" CHECK ("Status" IN (0, 1, 2));

-- First sign-in links by (case-insensitive) email, so an email may belong to only one user.
CREATE UNIQUE INDEX IF NOT EXISTS "UQ_AppUsers_Email"
    ON auth."AppUsers" (LOWER("Email"))
    WHERE "Email" IS NOT NULL;
