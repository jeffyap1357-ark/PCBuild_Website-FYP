-- Insert components into Component table. Skips rows where Name + CategoryId already exists.
-- ComponentId is auto-generated (not specified).

-- Motherboard (CategoryId 2)
INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[DdrType],[FormFactor],[Special],[RamSlots],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'ASUS B760-PLUS WIFI',2,899,N'Intel B760 ATX (2023). Gaming/office/editing. LGA1700, DDR5, PCIe 4.0. 4x RAM slots. M.2 heatsinks.',N'ASUS',N'b760_plus.jpg',N'LGA1700',N'DDR5',N'ATX',7,4,2023,4,75,7,8,7
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'ASUS B760-PLUS WIFI' AND [CategoryId]=2);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[DdrType],[FormFactor],[Special],[RamSlots],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'MSI PRO B760M-A WIFI',2,699,N'Intel B760 Micro-ATX (2023). Office/budget. LGA1700, DDR4, PCIe 4.0. 2x RAM slots.',N'MSI',N'b760m_pro.jpg',N'LGA1700',N'DDR4',N'Micro-ATX',6,2,2023,4,65,6,8,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'MSI PRO B760M-A WIFI' AND [CategoryId]=2);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[DdrType],[FormFactor],[Special],[RamSlots],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'MSI B550-A PRO',2,599,N'AMD B550 ATX (2020). Gaming/value. AM4, DDR4, PCIe 4.0. 4x RAM slots.',N'MSI',N'b550_pro.jpg',N'AM4',N'DDR4',N'ATX',6,4,2020,4,68,7,7,7
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'MSI B550-A PRO' AND [CategoryId]=2);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[DdrType],[FormFactor],[Special],[RamSlots],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'ASRock B650M Pro RS',2,999,N'AMD B650 Micro-ATX (2023). Gaming/editing. AM5, DDR5, PCIe 5.0. 4x RAM slots.',N'ASRock',N'b650m_rs.jpg',N'AM5',N'DDR5',N'Micro-ATX',8,4,2023,5,82,8,8,9
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'ASRock B650M Pro RS' AND [CategoryId]=2);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[DdrType],[FormFactor],[Special],[RamSlots],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Gigabyte B450M DS3H',2,399,N'AMD B450 Micro-ATX (2019). Budget/office. AM4, DDR4, PCIe 3.0. 2x RAM slots.',N'Gigabyte',N'b450m_ds3h.jpg',N'AM4',N'DDR4',N'Micro-ATX',5,2,2019,4,55,5,7,5
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Gigabyte B450M DS3H' AND [CategoryId]=2);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[DdrType],[FormFactor],[Special],[RamSlots],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'ASRock B560M Pro4',2,449,N'Intel B560 Micro-ATX (2021). Budget. LGA1200, DDR4, PCIe 4.0. 4x RAM slots.',N'ASRock',N'b560m_pro4.jpg',N'LGA1200',N'DDR4',N'Micro-ATX',6,4,2021,4,62,6,7,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'ASRock B560M Pro4' AND [CategoryId]=2);

-- CPU (CategoryId 1)
INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Intel Core i3-12100F',1,449,N'4-core 8-thread (2022). Budget gaming/office. LGA1700. 1080p gaming, light editing.',N'Intel',N'i3_12100f.jpg',N'LGA1700',6,2022,4,52,58,6,8,5
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Intel Core i3-12100F' AND [CategoryId]=1);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Intel Core i5-13400F',1,999,N'10-core 16-thread (2023). Mid-range gaming/editing. LGA1700. 1080p-1440p gaming.',N'Intel',N'i5_13400f.jpg',N'LGA1700',8,2023,5,78,65,8,8,8
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Intel Core i5-13400F' AND [CategoryId]=1);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Intel Core i7-13700K',1,1899,N'16-core 24-thread (2023). High-end. LGA1700. 4K gaming, heavy editing.',N'Intel',N'i7_13700k.jpg',N'LGA1700',10,2023,5,95,125,10,9,10
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Intel Core i7-13700K' AND [CategoryId]=1);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Ryzen 5 5500',1,449,N'6-core 12-thread (2022). Budget. AM4. 1080p gaming, office.',N'AMD',N'r5_5500.jpg',N'AM4',6,2022,4,55,65,6,8,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Ryzen 5 5500' AND [CategoryId]=1);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Ryzen 5 5600',1,699,N'6-core 12-thread (2022). Value gaming. AM4. 1080p gaming, light editing.',N'AMD',N'r5_5600.jpg',N'AM4',7,2022,5,72,65,7,8,7
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Ryzen 5 5600' AND [CategoryId]=1);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Ryzen 5 3600',1,499,N'6-core 12-thread (2019). Budget/older. AM4. 1080p gaming, office.',N'AMD',N'r5_3600.jpg',N'AM4',5,2019,4,58,65,6,7,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Ryzen 5 3600' AND [CategoryId]=1);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Ryzen 7 7700X',1,1699,N'8-core 16-thread (2023). High-end. AM5. 4K gaming, content creation.',N'AMD',N'r7_7700x.jpg',N'AM5',9,2023,5,92,105,9,9,10
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Ryzen 7 7700X' AND [CategoryId]=1);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Socket],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Intel Core i5-10400F',1,499,N'6-core 12-thread (2020). Budget/older. LGA1200. 1080p gaming, office.',N'Intel',N'i5_10400f.jpg',N'LGA1200',5,2020,4,55,65,6,7,5
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Intel Core i5-10400F' AND [CategoryId]=1);

-- GPU (CategoryId 3)
INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'GTX 1650 4GB',3,699,N'4GB GDDR6 (2019). Budget. 1080p low-med gaming, office.',N'NVIDIA',N'gtx1650.jpg',5,2019,4,45,75,5,7,4
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'GTX 1650 4GB' AND [CategoryId]=3);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'RX 6500 XT 4GB',3,799,N'4GB GDDR6 (2022). Budget. 1080p gaming, PCIe 4.0.',N'AMD',N'rx6500xt.jpg',5,2022,4,48,107,5,6,4
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'RX 6500 XT 4GB' AND [CategoryId]=3);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'RX 6600 8GB',3,1199,N'8GB GDDR6 (2021). Mid-range value. 1080p high, 1440p med gaming.',N'AMD',N'rx6600.jpg',7,2021,5,68,132,7,6,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'RX 6600 8GB' AND [CategoryId]=3);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'RTX 4060 8GB',3,1499,N'8GB GDDR6 (2023). Mid-range. 1080p-1440p gaming, DLSS3.',N'NVIDIA',N'rtx4060.jpg',8,2023,5,75,115,8,6,7
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'RTX 4060 8GB' AND [CategoryId]=3);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'RTX 4070 12GB',3,2599,N'12GB GDDR6X (2023). High-end. 1440p-4K gaming, editing.',N'NVIDIA',N'rtx4070.jpg',10,2023,5,92,200,10,7,9
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'RTX 4070 12GB' AND [CategoryId]=3);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'RX 7800 XT 16GB',3,2499,N'16GB GDDR6 (2023). High-end. 1440p-4K gaming, content creation.',N'AMD',N'rx7800xt.jpg',9,2023,5,90,263,9,7,9
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'RX 7800 XT 16GB' AND [CategoryId]=3);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'RTX 3060 12GB',3,1299,N'12GB GDDR6 (2021). Mid-range. 1080p-1440p, editing.',N'NVIDIA',N'rtx3060.jpg',7,2021,5,70,170,7,6,8
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'RTX 3060 12GB' AND [CategoryId]=3);

-- RAM (CategoryId 4)
INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[DdrType],[Special],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Kingston 8GB DDR4 3200MHz',4,149,N'8GB DDR4 (budget). Office, light use.',N'Kingston',N'kingston8ddr4.jpg',N'DDR4',6,4,35,4,7,3
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Kingston 8GB DDR4 3200MHz' AND [CategoryId]=4);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[DdrType],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Corsair 16GB DDR4 3200MHz',4,299,N'16GB DDR4 (2022). Gaming/office. 1080p gaming.',N'Corsair',N'corsair16ddr4.jpg',N'DDR4',6,2022,5,55,6,8,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Corsair 16GB DDR4 3200MHz' AND [CategoryId]=4);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[DdrType],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Kingston 16GB DDR5 5200MHz',4,399,N'16GB DDR5 (2023). Mid-range. DDR5 performance.',N'Kingston',N'kingston16ddr5.jpg',N'DDR5',7,2023,5,65,7,8,7
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Kingston 16GB DDR5 5200MHz' AND [CategoryId]=4);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[DdrType],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Corsair 32GB DDR4 3600MHz',4,599,N'32GB DDR4 (2022). Gaming/editing. High capacity.',N'Corsair',N'corsair32ddr4.jpg',N'DDR4',8,2022,5,75,8,8,9
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Corsair 32GB DDR4 3600MHz' AND [CategoryId]=4);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[DdrType],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Kingston 32GB DDR5 6000MHz',4,799,N'32GB DDR5 (2023). High-end. Content creation.',N'Kingston',N'kingston32ddr5.jpg',N'DDR5',9,2023,5,85,9,8,10
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Kingston 32GB DDR5 6000MHz' AND [CategoryId]=4);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[DdrType],[Special],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'G.Skill 16GB DDR4 2666MHz',4,229,N'16GB DDR4 (budget). Office, basic use.',N'G.Skill',N'gskill16ddr4.jpg',N'DDR4',5,4,45,5,7,4
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'G.Skill 16GB DDR4 2666MHz' AND [CategoryId]=4);

-- Storage (CategoryId 5)
INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Kingston NV2 500GB',5,199,N'500GB Gen4 NVMe (2022). Budget. Office, light gaming.',N'Kingston',N'nv2_500.jpg',5,2022,4,50,5,7,4
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Kingston NV2 500GB' AND [CategoryId]=5);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Samsung 970 EVO 1TB',5,499,N'1TB Gen3 NVMe (2021). Mid-range. Gaming, editing.',N'Samsung',N'970evo.jpg',8,2021,5,72,8,7,8
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Samsung 970 EVO 1TB' AND [CategoryId]=5);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'WD SN770 1TB',5,399,N'1TB Gen4 NVMe (2022). Value. Gaming, office.',N'Western Digital',N'sn770.jpg',7,2022,5,68,7,7,7
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'WD SN770 1TB' AND [CategoryId]=5);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Samsung 980 PRO 2TB',5,899,N'2TB Gen4 NVMe (2022). High-end. 4K editing, heavy workloads.',N'Samsung',N'980pro.jpg',9,2022,5,88,9,7,10
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Samsung 980 PRO 2TB' AND [CategoryId]=5);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Crucial P3 1TB',5,329,N'1TB Gen3 NVMe (2022). Budget. Gaming, office.',N'Crucial',N'p3_1tb.jpg',6,2022,4,60,6,7,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Crucial P3 1TB' AND [CategoryId]=5);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'WD Blue SN580 2TB',5,649,N'2TB Gen4 NVMe (2024). Mid-high. Content creation.',N'Western Digital',N'sn580_2tb.jpg',8,2024,5,80,8,7,9
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'WD Blue SN580 2TB' AND [CategoryId]=5);

-- PSU (CategoryId 6)
INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'FSP Hexa 550W 80+ White',6,199,N'550W (budget). Office, basic builds.',N'FSP',N'hexa550.jpg',4,3,40,NULL,4,6,4
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'FSP Hexa 550W 80+ White' AND [CategoryId]=6);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Corsair 650W 80+ Bronze',6,399,N'650W (2021). Mid-range. Gaming builds.',N'Corsair',N'corsair650.jpg',6,2021,4,60,NULL,6,7,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Corsair 650W 80+ Bronze' AND [CategoryId]=6);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Cooler Master 750W 80+ Gold',6,599,N'750W (2022). Mid-high. Gaming, editing.',N'Cooler Master',N'cm750.jpg',8,2022,5,78,NULL,8,7,8
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Cooler Master 750W 80+ Gold' AND [CategoryId]=6);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Seasonic 850W 80+ Gold',6,799,N'850W (2022). High-end. High-power GPUs.',N'Seasonic',N'seasonic850.jpg',9,2022,5,88,NULL,9,7,9
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Seasonic 850W 80+ Gold' AND [CategoryId]=6);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[ReleaseYear],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'be quiet! 750W 80+ Gold',6,649,N'750W (2023). Quiet, efficient. Gaming/editing.',N'be quiet',N'bq750.jpg',8,2023,5,82,NULL,8,7,8
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'be quiet! 750W 80+ Gold' AND [CategoryId]=6);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[Rating],[PerformanceScore],[PowerConsumption],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Thermaltake 550W 80+ Bronze',6,249,N'550W (budget). Office, entry gaming.',N'Thermaltake',N'tt550.jpg',5,4,50,NULL,5,6,5
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Thermaltake 550W 80+ Bronze' AND [CategoryId]=6);

-- CPU Cooler (CategoryId 7)
INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Noctua NH-D15',7,109.99,N'Dual tower, 140mm fans.',N'Noctua',N'noctua_nhd15.jpg',9,5,85,9,7,9
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Noctua NH-D15' AND [CategoryId]=7);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Cooler Master Hyper 212',7,44.99,N'Single tower, 120mm fan.',N'Cooler Master',N'hyper212.jpg',6,4,60,6,7,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Cooler Master Hyper 212' AND [CategoryId]=7);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[Special],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'be quiet! Dark Rock Pro 4',7,89.99,N'Dual tower, 135mm fan.',N'be quiet',N'darkrockpro4.jpg',8,5,80,8,7,8
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'be quiet! Dark Rock Pro 4' AND [CategoryId]=7);

-- PC Case (CategoryId 8)
INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[FormFactor],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'NZXT H5 Flow',8,399,N'ATX Mid Tower (2022). Gaming. Good airflow.',N'NZXT',N'nzxth5.jpg',N'ATX',7,2022,5,75,8,6,7
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'NZXT H5 Flow' AND [CategoryId]=8);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[FormFactor],[Special],[ReleaseYear],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Cooler Master TD300',8,299,N'Micro-ATX (2021). Compact. Office, SFF.',N'Cooler Master',N'td300.jpg',N'Micro-ATX',6,2021,4,65,6,7,6
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Cooler Master TD300' AND [CategoryId]=8);

INSERT INTO [Component] ([Name],[CategoryId],[Price],[Detail],[Brand],[Picture],[FormFactor],[Special],[Rating],[PerformanceScore],[GamingScore],[OfficeScore],[EditingScore])
SELECT N'Tecware Nexus M',8,199,N'Micro-ATX (budget). Office, value build.',N'Tecware',N'nexusm.jpg',N'Micro-ATX',5,4,55,5,7,5
WHERE NOT EXISTS (SELECT 1 FROM [Component] WHERE [Name]=N'Tecware Nexus M' AND [CategoryId]=8);
