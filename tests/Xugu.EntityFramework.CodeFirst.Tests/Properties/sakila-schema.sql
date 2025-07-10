USE SYSTEM;
GO
DROP DATABASE IF EXISTS `sakila`;
GO
CREATE DATABASE `sakila`;
GO
BEGIN
    IF EXISTS (SELECT 1 FROM `ALL_SCHEMAS` WHERE `SCHEMA_NAME` = 'sakila') THEN
        DROP SCHEMA sakila;
    END IF;
END;

GO
USE `sakila`;
GO

CREATE SCHEMA sakila;

GO

SET CURRENT_SCHEMA TO sakila

GO

--
-- Table structure for table `actor`
--

CREATE TABLE actor (
  actor_id INT  IDENTITY PRIMARY KEY NOT NULL,
  first_name VARCHAR(45) NOT NULL,
  last_name VARCHAR(45) NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

GO

CREATE INDEX idx_actor_last_name ON actor(last_name ASC);

GO

--
-- Table structure for table `country`
--

CREATE TABLE country (
  country_id INT  IDENTITY PRIMARY KEY NOT NULL,
  country VARCHAR(50) NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

GO

--
-- Table structure for table `city`
--

CREATE TABLE city (
  city_id INT IDENTITY PRIMARY KEY NOT NULL,
  city VARCHAR(50) NOT NULL,
  country_id INT  NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT `fk_city_country` FOREIGN KEY (country_id) REFERENCES country (country_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO

CREATE INDEX idx_fk_country_id ON city(country_id ASC);
--
-- Table structure for table `address`
--

CREATE TABLE address (
  address_id INT  IDENTITY PRIMARY KEY NOT NULL,
  address VARCHAR(50) NOT NULL,
  address2 VARCHAR(50) DEFAULT NULL,
  district VARCHAR(20) NOT NULL,
  city_id INT  NOT NULL,
  postal_code VARCHAR(10) DEFAULT NULL,
  phone VARCHAR(20) NOT NULL,
  /*!50705 location GEOMETRY NOT NULL,*/
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  /*!50705 SPATIAL KEY `idx_location` (location),*/
  CONSTRAINT `fk_address_city` FOREIGN KEY (city_id) REFERENCES city (city_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO

CREATE INDEX idx_fk_city_id ON address(city_id ASC);

GO
--
-- Table structure for table `category`
--

CREATE TABLE category (
  category_id INT  IDENTITY PRIMARY KEY NOT NULL,
  name VARCHAR(25) NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

GO
--
-- Table structure for table `staff`
--

CREATE TABLE staff (
  staff_id INT  IDENTITY PRIMARY KEY NOT NULL,
  first_name VARCHAR(45) NOT NULL,
  last_name VARCHAR(45) NOT NULL,
  address_id INT  NOT NULL,
  picture BLOB DEFAULT NULL,
  email VARCHAR(50) DEFAULT NULL,
  store_id INT  NOT NULL,
  active BOOLEAN NOT NULL DEFAULT TRUE,
  username VARCHAR(16) NOT NULL,
  password VARCHAR(40) DEFAULT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  --CONSTRAINT fk_staff_store FOREIGN KEY (store_id) REFERENCES store (store_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_staff_address FOREIGN KEY (address_id) REFERENCES address (address_id) ON DELETE RESTRICT ON UPDATE CASCADE
);
GO
CREATE INDEX idx_fk_store_id ON staff(store_id ASC);
CREATE INDEX idx_fk_address_id ON staff(address_id ASC);
GO
--
-- Table structure for table `store`
--

CREATE TABLE store (
  store_id INT  IDENTITY PRIMARY KEY NOT NULL,
  manager_staff_id INT UNIQUE NOT NULL,
  address_id INT  NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_store_staff FOREIGN KEY (manager_staff_id) REFERENCES staff (staff_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_store_address FOREIGN KEY (address_id) REFERENCES address (address_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO
CREATE INDEX idx_fk_address_id ON store(address_id ASC);
ALTER TABLE STAFF ADD CONSTRAINT fk_staff_store FOREIGN KEY (STORE_ID) REFERENCES STORE(STORE_ID);
GO
--
-- Table structure for table `customer`
--

CREATE TABLE customer (
  customer_id INT  IDENTITY PRIMARY KEY NOT NULL,
  store_id INT  NOT NULL,
  first_name VARCHAR(45) NOT NULL,
  last_name VARCHAR(45) NOT NULL,
  email VARCHAR(50) DEFAULT NULL,
  address_id INT  NOT NULL,
  active BOOLEAN NOT NULL DEFAULT TRUE,
  create_date DATETIME NOT NULL,
  last_update TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_customer_address FOREIGN KEY (address_id) REFERENCES address (address_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_customer_store FOREIGN KEY (store_id) REFERENCES store (store_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO
CREATE INDEX idx_fk_store_id ON customer(store_id ASC);
CREATE INDEX idx_fk_address_id ON customer(address_id ASC);
CREATE INDEX idx_last_name ON customer(last_name ASC);
GO

--
-- Table structure for table `language`
--

CREATE TABLE language (
  language_id INT  IDENTITY PRIMARY KEY NOT NULL,
  name CHAR(20) NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

GO
--
-- Table structure for table `film`
--

CREATE TABLE film (
  film_id INT  IDENTITY PRIMARY KEY NOT NULL,
  title VARCHAR(255) NOT NULL,
  description TEXT DEFAULT NULL,
  release_year INT DEFAULT NULL,
  language_id INT  NOT NULL,
  original_language_id INT  DEFAULT NULL,
  rental_duration INT  NOT NULL DEFAULT 3,
  rental_rate DECIMAL(4,2) NOT NULL DEFAULT 4.99,
  length INT  DEFAULT NULL,
  replacement_cost DECIMAL(5,2) NOT NULL DEFAULT 19.99,
  rating VARCHAR(255) DEFAULT 'G',
  special_features VARCHAR(255) DEFAULT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_film_language FOREIGN KEY (language_id) REFERENCES language (language_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_film_language_original FOREIGN KEY (original_language_id) REFERENCES language (language_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO
CREATE INDEX idx_title ON film(title ASC);
CREATE INDEX idx_fk_language_id ON film(language_id ASC);
CREATE INDEX idx_fk_original_language_id ON film(original_language_id ASC);
GO

--
-- Table structure for table `film_actor`
--

CREATE TABLE film_actor (
  actor_id INT PRIMARY KEY NOT NULL,
  film_id INT  NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_film_actor_actor FOREIGN KEY (actor_id) REFERENCES actor (actor_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_film_actor_film FOREIGN KEY (film_id) REFERENCES film (film_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO
CREATE INDEX idx_fk_film_id ON film_actor(film_id ASC);
GO
--
-- Table structure for table `film_category`
--

CREATE TABLE film_category (
  film_id INT PRIMARY KEY NOT NULL,
  category_id INT  NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_film_category_film FOREIGN KEY (film_id) REFERENCES film (film_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_film_category_category FOREIGN KEY (category_id) REFERENCES category (category_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO
--
-- Table structure for table `film_text`
-- 
-- InnoDB added FULLTEXT support in 5.6.10. If you use an
-- earlier version, then consider upgrading (recommended) or 
-- changing InnoDB to MyISAM as the film_text engine
--

CREATE TABLE film_text (
  film_id INT PRIMARY KEY NOT NULL,
  title VARCHAR(255) NOT NULL,
  description CLOB
);

GO
--
-- Triggers for loading film_text from film
--


CREATE TRIGGER `ins_film` AFTER INSERT ON `film` FOR EACH ROW BEGIN
    INSERT INTO film_text (film_id, title, description)
        VALUES (new.film_id, new.title, new.description);
  END;

GO

CREATE TRIGGER `upd_film` AFTER UPDATE ON `film` FOR EACH ROW BEGIN
    IF (old.title != new.title) OR (old.description != new.description) OR (old.film_id != new.film_id)
    THEN
        UPDATE film_text
            SET title=new.title,
                description=new.description,
                film_id=new.film_id
        WHERE film_id=old.film_id;
    END IF;
  END;

GO


CREATE TRIGGER `del_film` AFTER DELETE ON `film` FOR EACH ROW BEGIN
    DELETE FROM `film_text` WHERE film_id = old.film_id;
  END;

GO

--
-- Table structure for table `inventory`
--

CREATE TABLE inventory (
  inventory_id INT IDENTITY PRIMARY KEY NOT NULL,
  film_id INT  NOT NULL,
  store_id INT  NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_inventory_store FOREIGN KEY (store_id) REFERENCES store (store_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_inventory_film FOREIGN KEY (film_id) REFERENCES film (film_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO
CREATE INDEX idx_store_id_film_id ON inventory (store_id, film_id);
CREATE INDEX idx_fk_film_id ON inventory (film_id);
GO

--
-- Table structure for table `rental`
--

CREATE TABLE rental (
  rental_id INT IDENTITY PRIMARY KEY NOT NULL,
  rental_date DATETIME NOT NULL,
  inventory_id INT  NOT NULL,
  customer_id INT  NOT NULL,
  return_date DATETIME DEFAULT NULL,
  staff_id INT  NOT NULL,
  last_update TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE(rental_date,inventory_id,customer_id),
  CONSTRAINT fk_rental_staff FOREIGN KEY (staff_id) REFERENCES staff (staff_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_rental_inventory FOREIGN KEY (inventory_id) REFERENCES inventory (inventory_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_rental_customer FOREIGN KEY (customer_id) REFERENCES customer (customer_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO
CREATE INDEX idx_fk_inventory_id ON rental (inventory_id);
CREATE INDEX idx_fk_customer_id ON rental (customer_id);
CREATE INDEX idx_fk_staff_id ON rental (staff_id);
GO
--
-- Table structure for table `payment`
--

CREATE TABLE payment (
  payment_id INT IDENTITY PRIMARY KEY NOT NULL,
  customer_id INT  NOT NULL,
  staff_id INT  NOT NULL,
  rental_id INT DEFAULT NULL,
  amount DECIMAL(5,2) NOT NULL,
  payment_date DATETIME NOT NULL,
  last_update TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_payment_rental FOREIGN KEY (rental_id) REFERENCES rental (rental_id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_payment_customer FOREIGN KEY (customer_id) REFERENCES customer (customer_id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_payment_staff FOREIGN KEY (staff_id) REFERENCES staff (staff_id) ON DELETE RESTRICT ON UPDATE CASCADE
);

GO
CREATE INDEX idx_fk_customer_id ON payment (customer_id);
CREATE INDEX idx_fk_staff_id ON payment (staff_id);
GO



--
-- View structure for view `customer_list`
--

CREATE VIEW customer_list
AS
SELECT `cu`.`customer_id` AS ID, CONCAT(`cu`.`first_name`, ' ', `cu`.`last_name`) AS `NAME`,a.address AS address, a.postal_code AS `zip code`,
	a.phone AS phone, city.city AS city, country.country AS country, IF(`cu`.`active`, 'active','') AS `notes`, cu.store_id AS SID
FROM `customer` AS `cu` JOIN `address` AS `a` ON `cu`.`address_id` = `a`.`address_id` JOIN `city` ON `a`.`city_id` = `city`.`city_id`
	JOIN `country` ON `city`.`country_id` = `country`.`country_id`;

GO
--
-- View structure for view `film_list`
--

CREATE VIEW film_list
AS
WITH ActorList AS (
    SELECT 
        film.film_id,
        LISTAGG(CONCAT(actor.first_name, ' ', actor.last_name), ', ') AS actors
    FROM 
        film_actor
    JOIN 
        actor ON film_actor.actor_id = actor.actor_id
    JOIN film ON film_actor.film_id=film.film_id
    GROUP BY 
        film.film_id
)
SELECT 
    film.film_id AS FID, 
    film.title AS title, 
    film.description AS description, 
    `CATEGORY`.`NAME` AS `CATEGORY`, 
    film.rental_rate AS price,
    film.length AS `LENGTH`, 
    film.rating AS rating,
    ActorList.actors
FROM 
    `CATEGORY` 
LEFT JOIN 
    film_category ON `CATEGORY`.`category_id` = film_category.category_id 
LEFT JOIN 
    film ON film_category.film_id = film.film_id
LEFT JOIN 
    ActorList ON film.film_id = ActorList.film_id;

GO
--
-- View structure for view `nicer_but_slower_film_list`
--

CREATE VIEW nicer_but_slower_film_list
AS
WITH ActorList AS (
    SELECT 
        film.film_id, 
        LISTAGG(CONCAT(CONCAT(UPPER(SUBSTR(actor.first_name,1,1)),
	LOWER(SUBSTR(actor.first_name,2,LENGTH(actor.first_name))),' ',CONCAT(UPPER(SUBSTR(actor.last_name,1,1)),
	LOWER(SUBSTR(actor.last_name,2,LENGTH(actor.last_name)))))), ', ') AS actors
    FROM 
        film_actor
    JOIN 
        actor ON film_actor.actor_id = actor.actor_id
    JOIN 
        film ON film.film_id = film_actor.film_id
    GROUP BY 
        film.film_id
)
SELECT 
    film.film_id AS FID, 
    film.title AS title, 
    film.description AS description, 
    `CATEGORY`.`NAME` AS `CATEGORY`, 
    film.rental_rate AS price,
    `film`.`LENGTH` AS `LENGTH`, 
    film.rating AS rating, 
    ActorList.actors
FROM 
    category 
LEFT JOIN 
    film_category ON category.category_id = film_category.category_id 
LEFT JOIN 
    film ON film_category.film_id = film.film_id
LEFT JOIN 
    ActorList ON film.film_id = ActorList.film_id;

GO
--
-- View structure for view `staff_list`
--

CREATE VIEW staff_list
AS
SELECT s.staff_id AS ID, CONCAT(s.first_name,' ', s.last_name) AS name, a.address AS address, a.postal_code AS `zip code`, a.phone AS phone,
	city.city AS city, country.country AS country, s.store_id AS SID
FROM staff AS s JOIN address AS a ON s.address_id = a.address_id JOIN city ON a.city_id = city.city_id
	JOIN country ON city.country_id = country.country_id;

GO
--
-- View structure for view `sales_by_store`
--

CREATE VIEW sales_by_store
AS
WITH SalesData AS (
    SELECT
        CONCAT(c.city, ',', cy.country) AS store,
        CONCAT(m.first_name, ' ', m.last_name) AS manager,
        SUM(p.amount) AS total_sales,
        cy.country,
        c.city
    FROM payment AS p
    INNER JOIN rental AS r ON p.rental_id = r.rental_id
    INNER JOIN inventory AS i ON r.inventory_id = i.inventory_id
    INNER JOIN store AS s ON i.store_id = s.store_id
    INNER JOIN address AS a ON s.address_id = a.address_id
    INNER JOIN city AS c ON a.city_id = c.city_id
    INNER JOIN country AS cy ON c.country_id = cy.country_id
    INNER JOIN staff AS m ON s.manager_staff_id = m.staff_id
    GROUP BY s.store_id, cy.country, c.city, m.first_name, m.last_name
)
SELECT store, manager, total_sales
FROM SalesData
ORDER BY country, city;

GO
--
-- View structure for view `sales_by_film_category`
--
-- Note that total sales will add up to >100% because
-- some titles belong to more than 1 category
--

CREATE VIEW sales_by_film_category
AS
SELECT
c.name AS category
, SUM(p.amount) AS total_sales
FROM payment AS p
INNER JOIN rental AS r ON p.rental_id = r.rental_id
INNER JOIN inventory AS i ON r.inventory_id = i.inventory_id
INNER JOIN film AS f ON i.film_id = f.film_id
INNER JOIN film_category AS fc ON f.film_id = fc.film_id
INNER JOIN category AS c ON fc.category_id = c.category_id
GROUP BY c.name
ORDER BY total_sales DESC;

GO
--
-- View structure for view `actor_info`
--

CREATE VIEW actor_info
AS
SELECT
a.actor_id,
a.first_name,
a.last_name,
LISTAGG(DISTINCT CONCAT(c.name, ': ',
		(SELECT LISTAGG(f.title, ', ')
                    FROM sakila.film f
                    INNER JOIN sakila.film_category fc
                      ON f.film_id = fc.film_id
                    INNER JOIN sakila.film_actor fa
                      ON f.film_id = fa.film_id
                    WHERE fc.category_id = c.category_id
                    AND fa.actor_id = a.actor_id
                 )
             ), '; ')
AS film_info
FROM sakila.actor a
LEFT JOIN sakila.film_actor fa
  ON a.actor_id = fa.actor_id
LEFT JOIN sakila.film_category fc
  ON fa.film_id = fc.film_id
LEFT JOIN sakila.category c
  ON fc.category_id = c.category_id
GROUP BY a.actor_id, a.first_name, a.last_name;

GO
--
-- Procedure structure for procedure `rewards_report`
--


CREATE PROCEDURE rewards_report (
    min_monthly_purchases INT 
    ,min_dollar_amount_purchased DECIMAL(10,2) 
    ,count_rewardees OUT INT
) AS
	last_month_start DATE;
	last_month_end DATE;
BEGIN
	<<PROCBEGIN>>
    /* Some sanity checks... */
    IF min_monthly_purchases = 0 THEN
        SELECT 'Minimum monthly purchases parameter must be > 0';
        GOTO PROCBEGIN;
    END IF;
    IF min_dollar_amount_purchased = 0.00 THEN
        SELECT 'Minimum monthly dollar amount purchased parameter must be > $0.00';
        GOTO PROCBEGIN;
    END IF;

    /* Determine start and end time periods */
    last_month_start := (SELECT DATE_SUB(CURRENT_DATE(), INTERVAL 1 MONTH));
    last_month_start := (SELECT DATE_FORMAT(CONCAT(YEAR(last_month_start),'-',MONTH(last_month_start),'-01'),'%Y-%m-%d'));
    last_month_end := (SELECT LAST_DAY(last_month_start));

    
    SELECT COUNT(*) INTO count_rewardees FROM (SELECT p.customer_id
    FROM payment AS p
    WHERE DATE(p.payment_date) BETWEEN last_month_start AND last_month_end
    GROUP BY customer_id
    HAVING SUM(p.amount) > min_dollar_amount_purchased
    AND COUNT(customer_id) > min_monthly_purchases);

    /*
        Output ALL customer information of matching rewardees.
        Customize output as needed.
    */
    SELECT c.*
    FROM (SELECT p.customer_id
    FROM payment AS p
    WHERE DATE(p.payment_date) BETWEEN last_month_start AND last_month_end
    GROUP BY customer_id
    HAVING SUM(p.amount) > min_dollar_amount_purchased
    AND COUNT(customer_id) > min_monthly_purchases) AS t
    INNER JOIN customer AS c ON t.customer_id = c.customer_id;
END;

GO


CREATE FUNCTION get_customer_balance(p_customer_id INT, p_effective_date DATETIME) RETURN DECIMAL(5,2) AS
	v_rentfees DECIMAL(5,2);
	v_overfees INTEGER;
	v_payments DECIMAL(5,2);
BEGIN

  SELECT IFNULL(SUM(film.rental_rate),0) INTO v_rentfees
    FROM film, inventory, rental
    WHERE film.film_id = inventory.film_id
      AND inventory.inventory_id = rental.inventory_id
      AND rental.rental_date <= p_effective_date
      AND rental.customer_id = p_customer_id;

  SELECT IFNULL(SUM(IF((TO_DAYS(rental.return_date) - TO_DAYS(rental.rental_date)) > film.rental_duration,
        ((TO_DAYS(rental.return_date) - TO_DAYS(rental.rental_date)) - film.rental_duration),0)),0) INTO v_overfees
    FROM rental, inventory, film
    WHERE film.film_id = inventory.film_id
      AND inventory.inventory_id = rental.inventory_id
      AND rental.rental_date <= p_effective_date
      AND rental.customer_id = p_customer_id;


  SELECT IFNULL(SUM(payment.amount),0) INTO v_payments
    FROM payment

    WHERE payment.payment_date <= p_effective_date
    AND payment.customer_id = p_customer_id;

  RETURN v_rentfees + v_overfees - v_payments;
END;

GO


CREATE PROCEDURE film_in_stock(p_film_id INT,p_store_id INT, p_film_count OUT INT) AS
BEGIN
     SELECT inventory_id
     FROM inventory AS i
     WHERE film_id = p_film_id
     AND store_id = p_store_id
     AND ((SELECT COUNT(*) FROM rental AS r
    WHERE r.inventory_id = i.inventory_id)=0 OR
   (SELECT COUNT(r2.rental_id) FROM inventory AS i2 LEFT JOIN rental AS r2 ON (i2.inventory_id=r2.inventory_id)
    WHERE i2.inventory_id = i.inventory_id
    AND r2.return_date IS NULL)<=0);

     p_film_count:=SQL%ROWCOUNT;
END;

GO


CREATE PROCEDURE film_not_in_stock(p_film_id INT,p_store_id INT, p_film_count OUT INT) AS
BEGIN
     SELECT i.inventory_id
     FROM inventory AS i
     WHERE i.film_id = p_film_id
     AND i.store_id = p_store_id
     AND NOT ((SELECT COUNT(*) FROM rental AS r
    WHERE r.inventory_id = i.inventory_id)=0 OR
   (SELECT COUNT(r2.rental_id) FROM inventory AS i2 LEFT JOIN rental AS r2 ON (i2.inventory_id=r2.inventory_id)
    WHERE i2.inventory_id = i.inventory_id
    AND r2.return_date IS NULL)<=0);

     p_film_count:=SQL%ROWCOUNT;
END;

GO



CREATE FUNCTION inventory_held_by_customer(p_inventory_id INT) RETURN INT AS
v_customer_id INT;
BEGIN
  SELECT customer_id INTO v_customer_id
  FROM rental
  WHERE return_date IS NULL
  AND inventory_id = p_inventory_id;

  RETURN v_customer_id;
END;


GO



CREATE FUNCTION inventory_in_stock(p_inventory_id INT) RETURN BOOLEAN AS
v_rentals INT;
v_out     INT;
BEGIN
    SELECT COUNT(*) INTO v_rentals
    FROM rental
    WHERE inventory_id = p_inventory_id;

    IF v_rentals = 0 THEN
      RETURN TRUE;
    END IF;

    SELECT COUNT(rental_id) INTO v_out
    FROM inventory LEFT JOIN rental USING(inventory_id)
    WHERE inventory.inventory_id = p_inventory_id
    AND rental.return_date IS NULL;

    IF v_out > 0 THEN
      RETURN FALSE;
    ELSE
      RETURN TRUE;
    END IF;
END;


