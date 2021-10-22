CREATE TABLE IF NOT EXISTS refresh_tokens
(
	id bigint auto_increment
		primary key,
	user_id int null,
	token varchar(100) null,
	expires datetime null,
	date_created datetime null,
	created_by_ip varchar(30) null,
	date_revoked datetime null,
	revoked_by_ip varchar(30) null,
	replaced_by_token varchar(100) null,
	CONSTRAINT refresh_tokens_user_id
		FOREIGN KEY (user_id) REFERENCES users (id)
);