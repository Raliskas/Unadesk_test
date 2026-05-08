CREATE TABLE IF NOT EXISTS "Documents" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "FileName" VARCHAR(255) NOT NULL,
    "FilePath" VARCHAR(500) NOT NULL,
    "FileSize" BIGINT NOT NULL,
    "ContentType" VARCHAR(100) NOT NULL,
    "ExtractedText" TEXT,
    "Status" INT NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ProcessedAt" TIMESTAMP,
    "ErrorMessage" VARCHAR(255),
    
    CONSTRAINT "CHK_Documents_Status" CHECK ("Status" IN (0, 1, 2, 3))
);
