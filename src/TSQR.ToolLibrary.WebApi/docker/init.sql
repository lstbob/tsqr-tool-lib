-- TSQR Tool Library - Schema & Seed Data

CREATE TABLE IF NOT EXISTS Manufacturers (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(200) NOT NULL
);

CREATE TABLE IF NOT EXISTS Tools (
    Id SERIAL PRIMARY KEY,
    Model VARCHAR(200) NOT NULL,
    Description VARCHAR(1000) NOT NULL,
    ManufacturerId INTEGER NOT NULL REFERENCES Manufacturers(Id),
    ToolType INTEGER NOT NULL,
    AmortizationRate INTEGER NOT NULL,
    Metadata TEXT
);

CREATE TABLE IF NOT EXISTS Locations (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(200) NOT NULL
);

CREATE TABLE IF NOT EXISTS ToolScarcityByLocation (
    ToolId INTEGER NOT NULL REFERENCES Tools(Id),
    LocationId INTEGER NOT NULL REFERENCES Locations(Id),
    ScarcityLevel INTEGER NOT NULL,
    PRIMARY KEY (ToolId, LocationId)
);

CREATE TABLE IF NOT EXISTS Members (
    Id SERIAL PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    MiddleName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Age INTEGER NOT NULL,
    Address VARCHAR(500) NOT NULL,
    Email VARCHAR(200) NOT NULL,
    PhoneNumber VARCHAR(50) NOT NULL,
    Status INTEGER NOT NULL,
    IsVerified BOOLEAN NOT NULL,
    VerifiedByAdminId INTEGER REFERENCES Members(Id),
    VerificationDate TIMESTAMP,
    MembershipType INTEGER,
    StartDate TIMESTAMP,
    EndDate TIMESTAMP
);

CREATE TABLE IF NOT EXISTS InventoryItems (
    Id SERIAL PRIMARY KEY,
    ToolId INTEGER NOT NULL REFERENCES Tools(Id),
    OriginalOwnerId INTEGER NOT NULL REFERENCES Members(Id),
    InitialAcquisitionDate TIMESTAMP NOT NULL,
    SerialNumber VARCHAR(200) NOT NULL,
    Status INTEGER NOT NULL,
    Condition INTEGER NOT NULL,
    CurrentHolderId INTEGER REFERENCES Members(Id),
    LastBorrowedDate TIMESTAMP,
    LoanCount INTEGER NOT NULL DEFAULT 0,
    TotalUsageTimeTicks BIGINT NOT NULL DEFAULT 0,
    IsUnderRepair BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS Reservations (
    Id SERIAL PRIMARY KEY,
    ItemId INTEGER NOT NULL REFERENCES InventoryItems(Id),
    MemberId INTEGER NOT NULL REFERENCES Members(Id),
    ReservationDate TIMESTAMP NOT NULL,
    ExpiryDate TIMESTAMP NOT NULL,
    Status INTEGER NOT NULL,
    IsConfirmed BOOLEAN NOT NULL DEFAULT FALSE,
    QueuePosition INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS MaintenanceRecords (
    Id SERIAL PRIMARY KEY,
    ItemId INTEGER NOT NULL REFERENCES InventoryItems(Id),
    ReportedById INTEGER NOT NULL REFERENCES Members(Id),
    ReportedDate TIMESTAMP NOT NULL,
    Description VARCHAR(2000) NOT NULL,
    Status INTEGER NOT NULL,
    CompletedById INTEGER REFERENCES Members(Id),
    CompletedDate TIMESTAMP,
    ResultingCondition INTEGER
);

CREATE TABLE IF NOT EXISTS Loans (
    Id SERIAL PRIMARY KEY,
    MemberId INTEGER NOT NULL REFERENCES Members(Id),
    CheckoutDate TIMESTAMP NOT NULL,
    DueDate TIMESTAMP NOT NULL,
    ItemId INTEGER NOT NULL REFERENCES InventoryItems(Id),
    Status INTEGER NOT NULL,
    ReturnedDate TIMESTAMP,
    FineAccrued NUMERIC(10,2) NOT NULL DEFAULT 0
);

-- Seed: Manufacturers
INSERT INTO Manufacturers (Name) VALUES
    ('DeWalt'),
    ('Makita'),
    ('Bosch'),
    ('Stanley'),
    ('Milwaukee');

-- Seed: Locations
INSERT INTO Locations (Name) VALUES
    ('Downtown Workshop'),
    ('North Side Branch'),
    ('East End Hub'),
    ('Westside Station'),
    ('Central Warehouse');

-- Seed: Tools (ToolType: 1=HandTool,2=PowerTool,3=GardeningTool,4=ConstructionTool,5=SpecialtyTool,6=Other)
-- (AmortizationRate: 1=Low,2=Medium,3=High)
INSERT INTO Tools (Model, Description, ManufacturerId, ToolType, AmortizationRate, Metadata) VALUES
    ('Claw Hammer 16oz', 'Professional rip claw hammer with shock reduction grip', 1, 1, 1, NULL),
    ('Precision Screwdriver Set', '6-piece set with magnetic tips and ergonomic handle', 4, 1, 1, '{"pieces": 6}'),
    ('Cordless Drill 20V', '20V MAX cordless drill with 1/2" chuck and 2-speed transmission', 1, 2, 2, '{"voltage": 20, "chuck": "1/2"}'),
    ('Circular Saw 7-1/4"', '7-1/4 inch circular saw with electric brake and magnesium shoe', 2, 2, 3, '{"bladeSize": "7.25"}'),
    ('Round Point Shovel', 'Round point shovel with fiberglass handle and step treads', 4, 3, 1, NULL),
    ('Bow Rake', '14-tine bow rake with steel tines and 54-inch handle', 4, 3, 1, NULL),
    ('Torpedo Level 9"', '9-inch torpedo level with rare earth magnets', 3, 4, 1, NULL),
    ('Masonry Trowel', 'Philadelphia pattern brick trowel with hardwood handle', 4, 4, 1, NULL),
    ('Electric Pressure Washer', '1800 PSI electric pressure washer with onboard soap tank', 5, 5, 3, '{"psi": 1800}'),
    ('MIG Welder 140', '110V MIG welder with automatic feed and 6 heat settings', 1, 5, 3, '{"voltage": 110}'),
    ('Concrete Mixer 3.5 cu ft', '3.5 cubic foot portable concrete mixer with steel drum', 3, 5, 3, NULL),
    ('Coping Saw', 'Adjustable coping saw with 6-1/2 inch depth', 4, 1, 1, NULL),
    ('Angle Grinder 4-1/2"', '4-1/2 inch angle grinder with paddle switch and AC/DC switch', 2, 2, 2, NULL),
    ('Hedge Trimmer 22"', '22-inch dual-action hedge trimmer with rotating handle', 3, 3, 2, '{"bladeLength": 22}'),
    ('Measuring Tape 25ft', '25-foot measuring tape with fractional markings and blade lock', 4, 1, 1, '{"length": 25}'),
    ('Combination Wrench Set', '10-piece combination wrench set in SAE sizes', 1, 1, 1, '{"pieces": 10}'),
    ('Orbital Jigsaw', 'Variable-speed orbital jigsaw with tool-less blade change', 3, 2, 2, NULL),
    ('Paint Sprayer HVLP', 'HVLP paint sprayer with 3 pattern options and adjustable flow', 5, 5, 2, NULL),
    ('Wheelbarrow 6 cu ft', '6 cubic foot wheelbarrow with pneumatic tire and heavy-duty frame', 4, 3, 1, NULL),
    ('Portable Generator 5000W', '5000W portable generator with electric start and CO shutoff', 1, 5, 3, '{"watts": 5000}');

-- Seed: ToolScarcityByLocation (ScarcityLevel: 1=Low,2=Medium,3=High,4=Critical)
INSERT INTO ToolScarcityByLocation (ToolId, LocationId, ScarcityLevel) VALUES
    (1, 1, 2), (1, 2, 1), (1, 5, 1),
    (2, 1, 1), (2, 3, 2),
    (3, 1, 4), (3, 2, 3), (3, 3, 4), (3, 4, 3), (3, 5, 4),
    (4, 1, 2), (4, 5, 3),
    (5, 2, 1), (5, 4, 2),
    (6, 1, 1), (6, 3, 1),
    (7, 1, 2), (7, 3, 2), (7, 4, 1),
    (8, 2, 1),
    (9, 1, 2), (9, 5, 3),
    (10, 1, 4), (10, 5, 4),
    (11, 3, 3), (11, 4, 3),
    (13, 1, 2), (13, 2, 3), (13, 5, 2),
    (14, 2, 2), (14, 4, 3),
    (17, 1, 2), (17, 3, 1),
    (18, 1, 1), (18, 5, 2),
    (19, 2, 1), (19, 4, 1),
    (20, 1, 4), (20, 5, 4);

-- Seed: Members (Status: 1=Active,2=Suspended,3=Banned)
-- (MembershipType: 1=Regular,2=Repairman,3=LocationCoordinator,4=Admin)
INSERT INTO Members (FirstName, MiddleName, LastName, Age, Address, Email, PhoneNumber, Status, IsVerified, VerifiedByAdminId, VerificationDate, MembershipType, StartDate, EndDate) VALUES
    ('John', 'Michael', 'Smith', 34, '123 Oak St, Springfield, IL', 'john.smith@email.com', '555-0101', 1, TRUE, NULL, '2024-01-15 09:00:00', 4, '2024-01-15', NULL),
    ('Jane', 'Marie', 'Doe', 28, '456 Elm St, Springfield, IL', 'jane.doe@email.com', '555-0102', 1, TRUE, 1, '2024-02-01 10:30:00', 1, '2024-02-01', NULL),
    ('Robert', 'James', 'Johnson', 45, '789 Pine St, Springfield, IL', 'robert.j@email.com', '555-0103', 1, TRUE, 1, '2024-01-20 14:00:00', 2, '2024-01-20', NULL),
    ('Emily', 'Grace', 'Williams', 31, '321 Maple Ave, Springfield, IL', 'emily.w@email.com', '555-0104', 1, TRUE, 1, '2024-03-05 11:00:00', 1, '2024-03-05', NULL),
    ('Michael', 'David', 'Brown', 52, '654 Cedar Ln, Springfield, IL', 'mbrown@email.com', '555-0105', 1, TRUE, 1, '2024-01-10 08:00:00', 3, '2024-01-10', NULL),
    ('Sarah', 'Elizabeth', 'Davis', 26, '987 Birch Rd, Springfield, IL', 'sarah.davis@email.com', '555-0106', 1, FALSE, NULL, NULL, 1, '2024-06-01', '2024-12-01'),
    ('David', 'Alan', 'Wilson', 39, '147 Walnut Dr, Springfield, IL', 'dwilson@email.com', '555-0107', 2, TRUE, 1, '2024-03-15 09:30:00', 1, '2024-03-15', NULL),
    ('Lisa', 'Ann', 'Taylor', 41, '258 Spruce Ct, Springfield, IL', 'lisa.taylor@email.com', '555-0108', 1, TRUE, 1, '2024-04-01 13:00:00', 2, '2024-04-01', NULL),
    ('Kevin', 'Lee', 'Anderson', 33, '369 Ash Blvd, Springfield, IL', 'kevin.a@email.com', '555-0109', 3, TRUE, 1, '2024-02-20 10:00:00', 1, '2024-02-20', NULL),
    ('Amanda', 'Rose', 'Martinez', 29, '482 Hickory Way, Springfield, IL', 'amanda.m@email.com', '555-0110', 1, FALSE, NULL, NULL, 1, '2024-07-01', NULL);

-- Seed: InventoryItems (Status: 1=Available,2=Reserved,3=Loaned,4=UnderMaintenance,5=Lost)
-- (Condition: 1=New,2=Good,3=Fair,4=Repaired,5=Poor)
INSERT INTO InventoryItems (ToolId, OriginalOwnerId, InitialAcquisitionDate, SerialNumber, Status, Condition, CurrentHolderId, LastBorrowedDate, LoanCount, TotalUsageTimeTicks, IsUnderRepair) VALUES
    (1, 2, '2024-01-15 09:00:00', 'DW-HAM-001', 3, 2, 2, '2024-11-20 10:00:00', 8, 864000000000, FALSE),
    (3, 2, '2024-01-15 09:00:00', 'DW-DRL-001', 1, 1, NULL, NULL, 3, 144000000000, FALSE),
    (3, 3, '2024-02-01 10:00:00', 'DW-DRL-002', 4, 3, NULL, NULL, 5, 432000000000, TRUE),
    (4, 4, '2024-03-01 11:00:00', 'MK-SAW-001', 3, 2, 4, '2024-12-01 09:00:00', 2, 720000000000, FALSE),
    (5, 5, '2024-01-20 08:00:00', 'ST-SHV-001', 1, 2, NULL, NULL, 12, 864000000000, FALSE),
    (7, 2, '2024-04-01 14:00:00', 'BS-LVL-001', 2, 1, NULL, NULL, 0, 0, FALSE),
    (9, 8, '2024-05-01 09:00:00', 'ML-PW-001', 1, 1, NULL, NULL, 0, 0, FALSE),
    (10, 3, '2024-02-15 10:00:00', 'DW-WLD-001', 4, 2, NULL, NULL, 1, 288000000000, TRUE),
    (13, 4, '2024-06-01 11:00:00', 'MK-GRND-001', 1, 2, NULL, NULL, 4, 216000000000, FALSE),
    (14, 8, '2024-07-01 13:00:00', 'BS-HEDGE-001', 3, 1, 8, '2024-12-05 08:00:00', 1, 720000000000, FALSE),
    (17, 2, '2024-08-01 09:00:00', 'BS-JIG-001', 1, 1, NULL, NULL, 0, 0, FALSE),
    (19, 5, '2024-03-15 08:00:00', 'ST-WB-001', 1, 2, NULL, NULL, 10, 0, FALSE),
    (20, 5, '2024-09-01 10:00:00', 'DW-GEN-001', 1, 1, NULL, NULL, 0, 0, FALSE),
    (2, 10, '2024-10-01 09:00:00', 'ST-SCR-001', 1, 1, NULL, NULL, 0, 0, FALSE),
    (6, 10, '2024-10-01 09:00:00', 'ST-RAKE-001', 1, 1, NULL, NULL, 0, 0, FALSE);

-- Seed: Reservations (Status: 1=Pending,2=Confirmed,3=Active,4=Cancelled,5=Completed)
INSERT INTO Reservations (ItemId, MemberId, ReservationDate, ExpiryDate, Status, IsConfirmed, QueuePosition) VALUES
    (6, 6, '2024-12-10 09:00:00', '2024-12-20 09:00:00', 1, FALSE, 0),
    (6, 10, '2024-12-11 14:00:00', '2024-12-21 14:00:00', 1, FALSE, 1),
    (2, 4, '2024-12-12 10:00:00', '2024-12-22 10:00:00', 1, FALSE, 0),
    (7, 2, '2024-12-08 11:00:00', '2024-12-18 11:00:00', 5, TRUE, 0),
    (13, 6, '2024-12-09 08:00:00', '2024-12-19 08:00:00', 4, FALSE, 0),
    (9, 8, '2024-12-11 15:00:00', '2024-12-25 15:00:00', 1, FALSE, 0);

-- Seed: MaintenanceRecords (Status: 1=Reported,2=InProgress,3=Completed)
INSERT INTO MaintenanceRecords (ItemId, ReportedById, ReportedDate, Description, Status, CompletedById, CompletedDate, ResultingCondition) VALUES
    (3, 3, '2024-12-01 09:00:00', 'Chuck is slipping under load, needs replacement', 2, 8, NULL, NULL),
    (8, 3, '2024-12-05 10:00:00', 'Wire feed mechanism jams occasionally, needs cleaning and adjustment', 1, NULL, NULL, NULL),
    (1, 2, '2024-11-10 14:00:00', 'Handle cracked after heavy use, replaced handle and rebalanced', 3, 3, '2024-11-20 16:00:00', 4);
