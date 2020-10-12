-- database name = sqz[port][version][#questions]
CREATE TABLE IF NOT EXISTS `sqz_ver`(`id` INT UNSIGNED PRIMARY KEY);
DELETE FROM `sqz_ver`;
INSERT INTO `sqz_ver` VALUES (1000);

CREATE TABLE IF NOT EXISTS `sqz_date`(`dt` DATE PRIMARY KEY);

CREATE TABLE IF NOT EXISTS `sqz_status`(`id` TINYINT PRIMARY KEY);
INSERT INTO `sqz_status` VALUES (0),(1),(2);

CREATE TABLE IF NOT EXISTS `sqz_slot`(`dt` DATE, `t` TIME, `stt` TINYINT NOT NULL,
PRIMARY KEY(`dt`, `t`),
FOREIGN KEY(`dt`) REFERENCES `sqz_date`(`dt`),
FOREIGN KEY(`stt`) REFERENCES `sqz_status`(`id`),
INDEX(`stt`));

CREATE TABLE IF NOT EXISTS `sqz_room`(`id` TINYINT PRIMARY KEY);
INSERT INTO `sqz_room` VALUES (1),(2),(3),(4),(5),(6);

CREATE TABLE IF NOT EXISTS `sqz_slot_room`(`dt` DATE, `t` TIME, `rid` TINYINT,
`pw` CHAR(8) CHARACTER SET `ascii`,
`t1` TIME, `t2` TIME,
PRIMARY KEY(`dt`, `t`, `rid`),
FOREIGN KEY(`dt`, `t`) REFERENCES `sqz_slot`(`dt`, `t`),
FOREIGN KEY(`rid`) REFERENCES `sqz_room`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_qsheet`(`dt` DATE,
`t` TIME, `id` SMALLINT UNSIGNED,
PRIMARY KEY(`dt`, `id`),
FOREIGN KEY(`dt`, `t`) REFERENCES `sqz_slot`(`dt`, `t`);

CREATE TABLE IF NOT EXISTS `sqz_examinee`(`dt` DATE,
`id` CHAR(5)  CHARACTER SET `utf8mb4`, `t` TIME, `rid` TINYINT,
`name` VARCHAR(64) CHARACTER SET `utf8mb4`,
`birdate` DATE, `birthplace` VARCHAR(96) CHARACTER SET `utf8mb4`,
PRIMARY KEY(`dt`, `t`, `id`),
FOREIGN KEY(`dt`, `t`, `rid`) REFERENCES `sqz_slot_room`(`dt`, `t`, `rid`);

CREATE TABLE IF NOT EXISTS `sqz_nee_qsheet`(`dt` DATE, `subj` CHAR CHARACTER SET `ascii`,
`neeid` SMALLINT UNSIGNED, `qsid` SMALLINT UNSIGNED, `t1` TIME, `t2` TIME,
`grade` TINYINT, `comp` VARCHAR(32) CHARACTER SET `utf8mb4`,
`ans` CHAR(120) CHARACTER SET `ascii`, PRIMARY KEY(`dt`, `subj`, `neeid`),
FOREIGN KEY(`dt`, `subj`, `neeid`) REFERENCES `sqz_examinee`(`dt`, `subj`, `id`),
FOREIGN KEY(`dt`, `subj`, `qsid`) REFERENCES `sqz_qsheet`(`dt`, `subj`, `id`));

CREATE TABLE IF NOT EXISTS `sqz_module`(`id` TINYINT PRIMARY KEY);

CREATE TABLE IF NOT EXISTS `sqz_question`(`id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`moid` TINYINT, `del` TINYINT, `diff` TINYINT,
`stmt` VARCHAR(512) CHARACTER SET `utf8mb4`,
`ans0` VARCHAR(512) CHARACTER SET `utf8mb4`, `ans1` VARCHAR(512) CHARACTER SET `utf8mb4`,
`ans2` VARCHAR(512) CHARACTER SET `utf8mb4`, `ans3` VARCHAR(512) CHARACTER SET `utf8mb4`,
`key` CHAR(4) CHARACTER SET `ascii`, FOREIGN KEY(`moid`) REFERENCES `sqz_module`(`id`),
INDEX(`moid`,`del`,`diff`));

CREATE TABLE IF NOT EXISTS `sqz_qsheet_quest`(`dt` DATE, `subj` CHAR CHARACTER SET `ascii`,
`qsid` SMALLINT UNSIGNED, `qid` INT UNSIGNED, `asort` CHAR(4) CHARACTER SET `ascii`,
`idx` TINYINT,
PRIMARY KEY(`dt`, `subj`, `qsid`, `qid`),
FOREIGN KEY(`dt`, `subj`, `qsid`) REFERENCES `sqz_qsheet`(`dt`, `subj`, `id`),
FOREIGN KEY(`qid`) REFERENCES `sqz_question`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_img_path`(`id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`pat` VARCHAR(256) CHARACTER SET `utf8mb4`);

CREATE TABLE IF NOT EXISTS `sqz_quest_img`(`qid` INT UNSIGNED, `imgid` INT UNSIGNED,
`pos` INT, PRIMARY KEY(`qid`, `imgid`),
FOREIGN KEY(`qid`) REFERENCES `sqz_question`(`id`),
FOREIGN KEY(`imgid`) REFERENCES `sqz_img_path`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_admin`(`id` TINYINT PRIMARY KEY,
`name` VARCHAR(32) CHARACTER SET `utf8mb4`, `pw` CHAR(8) CHARACTER SET `ascii`);


INSERT INTO `sqz_module` VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14);
