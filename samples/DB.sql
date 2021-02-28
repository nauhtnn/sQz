-- proposed database name = sqz[port][version]
-- database name = sqzEN
CREATE TABLE IF NOT EXISTS `sqz_version`(`ver` INT);
INSERT INTO `sqz_version` VALUES (100);

CREATE TABLE IF NOT EXISTS `sqz_slot`(`dt` DATETIME, `status` INT,
PRIMARY KEY(`dt`));

CREATE TABLE IF NOT EXISTS `sqz_room`(`id` INT PRIMARY KEY);
INSERT INTO `sqz_room` VALUES (1),(2),(3),(4),(5),(6);

CREATE TABLE IF NOT EXISTS `sqz_slot_room`(`dt` DATETIME, `rid` INT,
`pw` CHAR(8) CHARACTER SET `ascii`,
`t1` TIME, `t2` TIME,
PRIMARY KEY(`dt`, `rid`),
FOREIGN KEY(`dt`) REFERENCES `sqz_slot`(`dt`),
FOREIGN KEY(`rid`) REFERENCES `sqz_room`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_qsheet`(`dt` DATETIME,
`id` INT,
PRIMARY KEY(`dt`, `id`),
FOREIGN KEY(`dt`) REFERENCES `sqz_slot`(`dt`));

CREATE TABLE IF NOT EXISTS `sqz_examinee`(`dt` DATETIME,
`id` VARCHAR(8) CHARACTER SET `utf8mb4`, `rid` INT,
`name` VARCHAR(64) CHARACTER SET `utf8mb4`,
`birthdate` VARCHAR(10), `birthplace` VARCHAR(96) CHARACTER SET `utf8mb4`,
PRIMARY KEY(`dt`, `id`),
FOREIGN KEY(`dt`, `rid`) REFERENCES `sqz_slot_room`(`dt`, `rid`));

CREATE TABLE IF NOT EXISTS `sqz_nee_qsheet`(`dt` DATETIME,
`neeid` VARCHAR(8) CHARACTER SET `utf8mb4`, `qsid` INT,
`t1` TIME, `t2` TIME, `grade` INT,
`comp` VARCHAR(32),
-- todo: Use TEXT
`ans` VARCHAR(1024),
FOREIGN KEY(`dt`, `neeid`) REFERENCES `sqz_examinee`(`dt`, `id`),
FOREIGN KEY(`dt`, `qsid`) REFERENCES `sqz_qsheet`(`dt`, `id`));

CREATE TABLE IF NOT EXISTS `sqz_passage`(`id` INT PRIMARY KEY,
`psg` TEXT);

CREATE TABLE IF NOT EXISTS `sqz_question`(`id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`pid` INT NULL, `deleted` INT,
-- todo: change to stem
`stmt` TEXT CHARACTER SET `utf8mb4`,
`ans0` TEXT CHARACTER SET `utf8mb4`, `ans1` TEXT CHARACTER SET `utf8mb4`,
`ans2` TEXT CHARACTER SET `utf8mb4`, `ans3` TEXT CHARACTER SET `utf8mb4`,
`akey` CHAR(4) CHARACTER SET `ascii`,
FOREIGN KEY(`pid`) REFERENCES `sqz_passage`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_qsheet_quest`(`dt` DATETIME,
`qsid`INT, `qid` INT UNSIGNED, `asort` CHAR(4) CHARACTER SET `ascii`,
`idx` INT,
PRIMARY KEY(`dt`, `qsid`, `qid`),
FOREIGN KEY(`dt`, `qsid`) REFERENCES `sqz_qsheet`(`dt`, `id`),
FOREIGN KEY(`qid`) REFERENCES `sqz_question`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_admin`(`name` VARCHAR(32) CHARACTER SET `utf8mb4` PRIMARY KEY,
`pw` CHAR(64) CHARACTER SET `ascii`);
