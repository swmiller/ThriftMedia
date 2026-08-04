-- Media inventory items belonging to a store
CREATE TABLE IF NOT EXISTS retail."Media"
(
    "MediaId"        SERIAL          PRIMARY KEY,
    "StoreId"        INTEGER         NOT NULL,
    "MediaType"      VARCHAR(50)     NOT NULL,
    "ImageUrl"       VARCHAR(500)    NOT NULL,
    "OcrPayloadJson" TEXT            NOT NULL,
    "IsTested"       BOOLEAN         NULL DEFAULT FALSE,
    "Price"          DECIMAL(18, 2)  NULL,
    "ShelfLocation"  VARCHAR(100)    NULL,
    "Condition"      VARCHAR(100)    NULL,

    -- Audit
    "CreatedBy"      VARCHAR(100)    NOT NULL,
    "CreatedAt"      TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"      VARCHAR(100)    NULL,
    "UpdatedAt"      TIMESTAMP       NULL,

    CONSTRAINT "FK_Media_Store" FOREIGN KEY ("StoreId")
        REFERENCES retail."Store" ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Media_StoreId"
    ON retail."Media" ("StoreId");
