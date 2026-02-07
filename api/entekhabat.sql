-- phpMyAdmin SQL Dump
-- version 5.1.3
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Feb 07, 2026 at 01:11 AM
-- Server version: 10.4.24-MariaDB
-- PHP Version: 7.4.29

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `entekhabat`
--

-- --------------------------------------------------------

--
-- Table structure for table `advertisements`
--

CREATE TABLE `advertisements` (
  `id` int(11) NOT NULL,
  `nationalId` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_persian_ci NOT NULL,
  `title` text NOT NULL,
  `create_date` timestamp NOT NULL DEFAULT current_timestamp(),
  `description` text NOT NULL,
  `image` varchar(200) NOT NULL,
  `type` varchar(30) NOT NULL,
  `status` varchar(20) NOT NULL,
  `start_date` varchar(20) DEFAULT NULL,
  `end_date` varchar(20) DEFAULT NULL,
  `views` int(11) NOT NULL,
  `target_link` varchar(200) DEFAULT NULL,
  `updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `deleter` varchar(15) DEFAULT NULL,
  `reson` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `advertisements`
--

INSERT INTO `advertisements` (`id`, `nationalId`, `title`, `create_date`, `description`, `image`, `type`, `status`, `start_date`, `end_date`, `views`, `target_link`, `updated_at`, `deleter`, `reson`) VALUES
(3, '0534921972', 'عنوان تبلیغ', '2026-02-06 15:07:58', 'متن تبلیغ', 'uploads/advertisements/ad_20260206_185234_a69ccc0a.png', 'banner', 'active', '2026-02-06', '2026-03-08', 1, 'https://www.a.com', '2026-02-06 17:22:46', NULL, NULL),
(4, '0534921972', 'عنوان تبلیغ1', '2026-02-06 15:24:38', 'متن تبلیغ\r\n111', 'uploads/advertisements/ad_20260206_185438_2e30a3e2.png', 'video', 'active', NULL, NULL, 3, 'https://www.a.com', '2026-02-06 17:22:48', NULL, NULL),
(5, '0534921972', 'عنوان تبلیغ12', '2026-02-06 15:25:07', 'متن تبلیغ2', 'uploads/advertisements/ad_20260206_185507_fb4262ed.png', 'text', 'active', NULL, NULL, 2, 'https://www.a.com', '2026-02-06 17:55:10', 'CANDIDATE', ''),
(6, '0534921972', 'عنوان تبلیغ13', '2026-02-06 15:25:23', 'متن تبلیغ\r\n22', 'uploads/advertisements/ad_20260206_185523_00b35e17.png', 'popup', 'active', NULL, NULL, 7, 'https://www.a.com', '2026-02-06 17:29:33', 'SUPERVISOR', '123');

-- --------------------------------------------------------

--
-- Table structure for table `config`
--

CREATE TABLE `config` (
  `id` int(11) NOT NULL,
  `startDate` timestamp NULL DEFAULT NULL,
  `EndDate` timestamp NULL DEFAULT NULL,
  `create_date` timestamp NOT NULL DEFAULT current_timestamp(),
  `active` tinyint(4) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `config`
--

INSERT INTO `config` (`id`, `startDate`, `EndDate`, `create_date`, `active`) VALUES
(1, '2026-02-06 04:30:00', '2026-02-07 18:30:00', '2026-02-06 18:07:17', 1);

-- --------------------------------------------------------

--
-- Table structure for table `final_submissions`
--

CREATE TABLE `final_submissions` (
  `id` int(11) NOT NULL,
  `nationalId` varchar(20) NOT NULL,
  `tracking_code` varchar(100) NOT NULL,
  `requestStatus` varchar(30) NOT NULL,
  `reson` text DEFAULT NULL,
  `create_date` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `final_submissions`
--

INSERT INTO `final_submissions` (`id`, `nationalId`, `tracking_code`, `requestStatus`, `reson`, `create_date`) VALUES
(4, '0534921972', '21A7A722', 'SUPERVISION_APPROVED', '111', '2026-02-06 02:42:26'),
(5, '0534921971', '21A7A723', 'SUPERVISION_APPROVED', '', '2026-02-06 03:42:26');

-- --------------------------------------------------------

--
-- Table structure for table `logs`
--

CREATE TABLE `logs` (
  `id` int(11) NOT NULL,
  `nationalId` varchar(20) NOT NULL,
  `action` varchar(150) NOT NULL,
  `create_date` timestamp NOT NULL DEFAULT current_timestamp(),
  `description` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `logs`
--

INSERT INTO `logs` (`id`, `nationalId`, `action`, `create_date`, `description`) VALUES
(2, '0534921973', 'تغییر وضعیت', '2026-02-06 13:48:12', 'تغییر کدملی 0534921972 به SUPERVISION_REJECTED'),
(3, '0534921973', 'تغییر وضعیت', '2026-02-06 13:49:05', 'تغییر کدملی 0534921972 به SUPERVISION_APPROVED'),
(4, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 16:39:29', 'تغییر کد 2 به 1'),
(5, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 16:40:13', 'تغییر کد 2 به 1'),
(6, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 16:40:49', 'تغییر کد 2 به 1'),
(7, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 16:42:39', 'تغییر کد 6 به 1'),
(8, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 16:49:49', 'تغییر کد 6 به 1'),
(9, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 16:50:39', 'تغییر کد 6 به 1'),
(10, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 16:51:26', 'تغییر کد 6 به 0'),
(11, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 16:51:52', 'تغییر کد 6 به 1'),
(12, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:03:31', 'تغییر کد 5 به 1'),
(13, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:03:41', 'تغییر کد 6 به 0'),
(14, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:18:15', 'تغییر کد 5 به 0'),
(15, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:18:33', 'تغییر کد 5 به 1'),
(16, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:19:55', 'تغییر کد 5 به 0'),
(17, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:20:09', 'تغییر کد 5 به 1'),
(18, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:25:41', 'تغییر کد 6 به SUPERVISOR'),
(19, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:26:17', 'تغییر کد 6 به 0'),
(20, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:27:33', 'تغییر کد 6 به SUPERVISOR'),
(21, '0534921973', 'تغییر وضعیت تبلیغ', '2026-02-06 17:27:53', 'تغییر کد 6 به '),
(22, '0534921972', 'تغییر وضعیت تبلیغ', '2026-02-06 17:55:10', 'تغییر کد 5 به CANDIDATE'),
(23, '0534921972', 'ثبت رای', '2026-02-06 21:08:46', ' کد 0534921972 به 5 رای داد'),
(24, '0534921972', 'ثبت رای', '2026-02-07 00:08:11', ' کد 0534921972 به 2 رای داد');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `id` bigint(20) NOT NULL,
  `national_id` varchar(10) COLLATE utf8mb4_persian_ci NOT NULL,
  `first_name` varchar(50) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `last_name` varchar(50) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `father_name` varchar(50) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `gender` tinyint(4) DEFAULT NULL,
  `birth_date` date DEFAULT NULL,
  `persian_birth_date` varchar(10) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `mobile` varchar(11) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `user_type` int(11) DEFAULT NULL,
  `verified` tinyint(1) DEFAULT NULL,
  `employee_key` char(36) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `personnel_code` int(11) DEFAULT NULL,
  `org_position_code` int(11) DEFAULT NULL,
  `org_position_desc` varchar(255) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `org_position_type_code` int(11) DEFAULT NULL,
  `org_position_type_desc` varchar(50) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `region_id` int(11) DEFAULT NULL,
  `is_foreigner` tinyint(1) DEFAULT NULL,
  `ip_address` varchar(45) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NULL DEFAULT NULL,
  `roles` varchar(200) COLLATE utf8mb4_persian_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_persian_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `national_id`, `first_name`, `last_name`, `father_name`, `gender`, `birth_date`, `persian_birth_date`, `mobile`, `user_type`, `verified`, `employee_key`, `personnel_code`, `org_position_code`, `org_position_desc`, `org_position_type_code`, `org_position_type_desc`, `region_id`, `is_foreigner`, `ip_address`, `created_at`, `updated_at`, `roles`) VALUES
(2, '0534921972', 'ميلاد', 'فراهاني', 'صفرعلي', 1, '1987-01-26', '1365/11/06', '09186283451', 3, 0, 'f904d5ac-41b0-4841-b9a7-a38a44febf32', 96304232, 10000541, 'کارشناس بهبودکیفیت و توسعه برنامه های آموزشی نوین وهوشمند سازی مدارس', 4, 'کارشناس', 1000, 0, '2.178.30.132', '2026-02-05 17:23:14', '2026-02-06 18:02:11', 'CANDIDATE'),
(4, '0534921973', 'ميلاد', 'فراهاني', 'صفرعلي', 1, '1987-01-26', '1365/11/06', '09186283451', 3, 0, 'f904d5ac-41b0-4841-b9a7-a38a44febf32', 96304232, 10000541, 'کارشناس بهبودکیفیت و توسعه برنامه های آموزشی نوین وهوشمند سازی مدارس', 4, 'کارشناس', 1000, 0, '2.178.30.132', '2026-02-05 17:23:14', '2026-02-06 17:55:57', 'EXECUTIVE'),
(5, '0534921971', 'ميلاد1', 'فراهاني2', 'صفرعلي', 1, '1987-01-26', '1365/11/06', '09186283451', 3, 0, 'f904d5ac-41b0-4841-b9a7-a38a44febf32', 96304232, 10000541, 'کارشناس بهبودکیفیت و توسعه برنامه های آموزشی نوین وهوشمند سازی مدارس', 4, 'کارشناس', 1000, 0, '2.178.30.132', '2026-02-05 17:23:14', '2026-02-06 00:19:09', 'CANDIDATE');

-- --------------------------------------------------------

--
-- Table structure for table `userstatus`
--

CREATE TABLE `userstatus` (
  `user_id` bigint(20) NOT NULL,
  `nationalId` varchar(20) NOT NULL,
  `ozvsandogh` tinyint(1) DEFAULT NULL,
  `sabegheO` tinyint(1) DEFAULT NULL,
  `madrak` tinyint(1) DEFAULT NULL,
  `create_date` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `userstatus`
--

INSERT INTO `userstatus` (`user_id`, `nationalId`, `ozvsandogh`, `sabegheO`, `madrak`, `create_date`) VALUES
(2, '0534921972', 1, 1, 1, '2026-02-05 22:42:42'),
(4, '0534921973', 1, 1, 1, '2026-02-06 00:52:33');

-- --------------------------------------------------------

--
-- Table structure for table `user_addresses`
--

CREATE TABLE `user_addresses` (
  `id` bigint(20) NOT NULL,
  `user_id` bigint(20) NOT NULL,
  `post_code` varchar(10) COLLATE utf8mb4_persian_ci DEFAULT NULL,
  `address` text COLLATE utf8mb4_persian_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_persian_ci;

--
-- Dumping data for table `user_addresses`
--

INSERT INTO `user_addresses` (`id`, `user_id`, `post_code`, `address`) VALUES
(1, 2, '1764973127', 'استان تهران،شهر تهران،شیوا،خیابان ش حجت الاسلام مهدی شاه آبادی،کوچه اعتصامی، کد پستی ١٧٦٤٩٧٣١٢٧'),
(2, 4, '1764973127', 'استان تهران،شهر تهران،شیوا،خیابان ش حجت الاسلام مهدی شاه آبادی،کوچه اعتصامی، کد پستی ١٧٦٤٩٧٣١٢٧');

-- --------------------------------------------------------

--
-- Table structure for table `user_documents`
--

CREATE TABLE `user_documents` (
  `id` int(11) NOT NULL,
  `nationalId` varchar(20) DEFAULT NULL,
  `user_photo` varchar(255) NOT NULL,
  `education_doc` varchar(255) NOT NULL,
  `employment_cert` varchar(255) NOT NULL,
  `created_at` datetime DEFAULT current_timestamp(),
  `updated_at` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `user_documents`
--

INSERT INTO `user_documents` (`id`, `nationalId`, `user_photo`, `education_doc`, `employment_cert`, `created_at`, `updated_at`) VALUES
(2, '0534921972', 'uploads/user_documents/nid_0534921973/user_photo_20260206_0341_6a5065be.png', 'uploads/user_documents/nid_0534921973/education_doc_20260206_0341_c3444d3c.png', 'uploads/user_documents/nid_0534921973/employment_cert_20260206_0341_75081aa3.png', '2026-02-06 03:01:47', '2026-02-06 04:22:02'),
(6, '0534921971', 'uploads/user_documents/nid_0534921973/user_photo_20260206_0341_6a5065be.png', 'uploads/user_documents/nid_0534921973/education_doc_20260206_0341_c3444d3c.png', 'uploads/user_documents/nid_0534921973/employment_cert_20260206_0341_75081aa3.png', '2026-02-06 03:01:47', '2026-02-06 04:22:02');

-- --------------------------------------------------------

--
-- Table structure for table `voters`
--

CREATE TABLE `voters` (
  `id` int(11) NOT NULL,
  `candidateId` bigint(20) NOT NULL,
  `createdate` timestamp NOT NULL DEFAULT current_timestamp(),
  `usernationalid` varchar(15) NOT NULL,
  `trackingCode` varchar(10) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `voters`
--

INSERT INTO `voters` (`id`, `candidateId`, `createdate`, `usernationalid`, `trackingCode`) VALUES
(1, 5, '2026-02-06 21:08:46', '05349219721', 'VT12126127'),
(4, 2, '2026-02-07 00:08:11', '0534921972', 'VT22891182');

-- --------------------------------------------------------

--
-- Table structure for table `voting_tokens`
--

CREATE TABLE `voting_tokens` (
  `id` bigint(20) NOT NULL,
  `user_id` varchar(20) NOT NULL,
  `token_hash` varchar(255) NOT NULL,
  `expires_at` datetime NOT NULL,
  `used` tinyint(4) DEFAULT 0,
  `used_at` datetime DEFAULT NULL,
  `created_at` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `voting_tokens`
--

INSERT INTO `voting_tokens` (`id`, `user_id`, `token_hash`, `expires_at`, `used`, `used_at`, `created_at`) VALUES
(6, '0534921972', '3316377c35a02808ac40a1069b6e08861f48a9447282b9a6c5d675f11b654fcb', '2026-02-07 03:40:11', 1, '2026-02-07 03:38:11', '2026-02-07 03:38:11');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `advertisements`
--
ALTER TABLE `advertisements`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `config`
--
ALTER TABLE `config`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `final_submissions`
--
ALTER TABLE `final_submissions`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uniq_final_submit_national_id` (`nationalId`);

--
-- Indexes for table `logs`
--
ALTER TABLE `logs`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `national_id` (`national_id`);

--
-- Indexes for table `userstatus`
--
ALTER TABLE `userstatus`
  ADD PRIMARY KEY (`user_id`);

--
-- Indexes for table `user_addresses`
--
ALTER TABLE `user_addresses`
  ADD PRIMARY KEY (`id`),
  ADD KEY `user_id` (`user_id`);

--
-- Indexes for table `user_documents`
--
ALTER TABLE `user_documents`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uniq_user_docs_national_id` (`nationalId`);

--
-- Indexes for table `voters`
--
ALTER TABLE `voters`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `voting_tokens`
--
ALTER TABLE `voting_tokens`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_token` (`token_hash`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `advertisements`
--
ALTER TABLE `advertisements`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `config`
--
ALTER TABLE `config`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `final_submissions`
--
ALTER TABLE `final_submissions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `logs`
--
ALTER TABLE `logs`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=25;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `user_addresses`
--
ALTER TABLE `user_addresses`
  MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `user_documents`
--
ALTER TABLE `user_documents`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `voters`
--
ALTER TABLE `voters`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `voting_tokens`
--
ALTER TABLE `voting_tokens`
  MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `user_addresses`
--
ALTER TABLE `user_addresses`
  ADD CONSTRAINT `user_addresses_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
