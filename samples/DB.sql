-- database name = sqz[port][version]
CREATE TABLE IF NOT EXISTS `sqz_version`(`ver` INT UNSIGNED);
INSERT INTO `sqz_version` VALUES (100);

CREATE TABLE IF NOT EXISTS `sqz_slot`(`dt` DATETIME, `status` TINYINT,
PRIMARY KEY(`dt`));

CREATE TABLE IF NOT EXISTS `sqz_room`(`id` TINYINT PRIMARY KEY);
INSERT INTO `sqz_room` VALUES (1),(2),(3),(4),(5),(6);

CREATE TABLE IF NOT EXISTS `sqz_slot_room`(`dt` DATETIME, `rid` TINYINT,
`pw` CHAR(8) CHARACTER SET `ascii`,
`t1` TIME, `t2` TIME,
PRIMARY KEY(`dt`, `rid`),
FOREIGN KEY(`dt`) REFERENCES `sqz_slot`(`dt`),
FOREIGN KEY(`rid`) REFERENCES `sqz_room`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_qsheet`(`dt` DATETIME,
`id` SMALLINT UNSIGNED,
PRIMARY KEY(`dt`, `id`),
FOREIGN KEY(`dt`) REFERENCES `sqz_slot`(`dt`));

CREATE TABLE IF NOT EXISTS `sqz_examinee`(`dt` DATETIME,
`id` CHAR(5)  CHARACTER SET `utf8mb4`, `rid` TINYINT,
`name` VARCHAR(64) CHARACTER SET `utf8mb4`,
`birdate` VARCHAR(10), `birthplace` VARCHAR(96) CHARACTER SET `utf8mb4`,
`qsid` SMALLINT UNSIGNED,
`t1` TIME, `t2` TIME, `grade` SMALLINT UNSIGNED,
`comp` VARCHAR(32),
`ans` VARCHAR(1024),
PRIMARY KEY(`dt`, `id`),
FOREIGN KEY(`dt`, `rid`) REFERENCES `sqz_slot_room`(`dt`, `rid`),
FOREIGN KEY(`dt`, `qsid`) REFERENCES `sqz_qsheet`(`dt`, `id`));

CREATE TABLE IF NOT EXISTS `sqz_passage`(`id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`psg` TEXT);

CREATE TABLE IF NOT EXISTS `sqz_question`(`id` INT UNSIGNED PRIMARY KEY,
`pid` INT UNSIGNED,
`stmt` TEXT CHARACTER SET `utf8mb4`,
`ans0` TEXT CHARACTER SET `utf8mb4`, `ans1` TEXT CHARACTER SET `utf8mb4`,
`ans2` TEXT CHARACTER SET `utf8mb4`, `ans3` TEXT CHARACTER SET `utf8mb4`,
`akey` CHAR(4) CHARACTER SET `ascii`,
FOREIGN KEY(`pid`) REFERENCES `sqz_passage`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_qsheet_quest`(`dt` DATETIME,
`qsid` SMALLINT UNSIGNED, `qid` INT UNSIGNED, `asort` CHAR(4) CHARACTER SET `ascii`,
`idx` TINYINT,
PRIMARY KEY(`dt`, `qsid`, `qid`),
FOREIGN KEY(`dt`, `qsid`) REFERENCES `sqz_qsheet`(`dt`, `id`),
FOREIGN KEY(`qid`) REFERENCES `sqz_question`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_admin`(`name` VARCHAR(32) CHARACTER SET `utf8mb4` PRIMARY KEY,
`pw` CHAR(64) CHARACTER SET `ascii`);
