-- proposed database name = sqz[port][version]
-- database name = sqzEN
CREATE TABLE IF NOT EXISTS `sqz_version`(`ver` INT);
INSERT INTO `sqz_version` VALUES (200);

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

CREATE TABLE IF NOT EXISTS `sqz_subject`(`id` INT PRIMARY KEY);

CREATE TABLE IF NOT EXISTS `sqz_qsheet`(`dt` DATETIME,
`id` INT, `subj_id` INT,
PRIMARY KEY(`dt`, `id`),
FOREIGN KEY(`dt`) REFERENCES `sqz_slot`(`dt`),
FOREIGN KEY(`subj_id`) REFERENCES `sqz_subject`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_examinee`(`dt` DATETIME,
`id` VARCHAR(8) CHARACTER SET `utf8mb4`, `rid` INT,
`name` VARCHAR(64) CHARACTER SET `utf8mb4`,
`birthdate` VARCHAR(10), `subj_id` INT,
PRIMARY KEY(`dt`, `id`),
FOREIGN KEY(`dt`, `rid`) REFERENCES `sqz_slot_room`(`dt`, `rid`),
FOREIGN KEY(`subj_id`) REFERENCES `sqz_subject`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_nee_qsheet`(`dt` DATETIME,
`neeid` VARCHAR(8) CHARACTER SET `utf8mb4`, `qsid` INT,
`t1` TIME, `t2` TIME, `grade` INT,
`comp` VARCHAR(32),
`ans` TEXT,
FOREIGN KEY(`dt`, `neeid`) REFERENCES `sqz_examinee`(`dt`, `id`),
FOREIGN KEY(`dt`, `qsid`) REFERENCES `sqz_qsheet`(`dt`, `id`));

CREATE TABLE IF NOT EXISTS `sqz_sec_type`(`id` INT PRIMARY KEY,
`name` VARCHAR(32));

INSERT INTO `sqz_sec_type` VALUES (0, 'DefaultIndependentQuestions'), (1, 'MTFIndependentQuestions'), (2, 'BasicPassage'), (3, 'PassageWithBlanks');

CREATE TABLE IF NOT EXISTS `sqz_section`(`id` INT PRIMARY KEY,
`s_type` INT, `req` TEXT CHARACTER SET `utf8mb4`,
`psg` TEXT CHARACTER SET `utf8mb4`, `config` TEXT CHARACTER SET `utf8mb4`,
FOREIGN KEY(`s_type`) REFERENCES `sqz_sec_type`(`id`));

INSERT INTO `sqz_section` VALUES (0, 0, 'initial req', 'initial psg', 'initial config'), (1, 1, 'initial req', 'initial psg', 'initial config');

CREATE TABLE IF NOT EXISTS `sqz_question`(`id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
`subj_id` INT,
`secid` INT,
`deleted` INT,
`quest_type` INT,
`stem` TEXT CHARACTER SET `utf8mb4`,
`ans0` TEXT CHARACTER SET `utf8mb4`, `ans1` TEXT CHARACTER SET `utf8mb4`,
`ans2` TEXT CHARACTER SET `utf8mb4`, `ans3` TEXT CHARACTER SET `utf8mb4`,
`akey` CHAR(4) CHARACTER SET `ascii`,
FOREIGN KEY(`subj_id`) REFERENCES `sqz_subject`(`id`),
FOREIGN KEY(`secid`) REFERENCES `sqz_section`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_qsheet_quest`(`dt` DATETIME,
`qsid`INT, `qid` INT UNSIGNED, `asort` CHAR(4) CHARACTER SET `ascii`,
`idx` INT,
PRIMARY KEY(`dt`, `qsid`, `qid`),
FOREIGN KEY(`dt`, `qsid`) REFERENCES `sqz_qsheet`(`dt`, `id`),
FOREIGN KEY(`qid`) REFERENCES `sqz_question`(`id`));

CREATE TABLE IF NOT EXISTS `sqz_admin`(`name` VARCHAR(32) CHARACTER SET `utf8mb4` PRIMARY KEY,
`pw` CHAR(64) CHARACTER SET `ascii`);
