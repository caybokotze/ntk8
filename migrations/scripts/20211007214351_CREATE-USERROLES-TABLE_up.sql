CREATE TABLE user_roles
(
    id      int PRIMARY KEY AUTO_INCREMENT,
    user_id int,
    role_id int
);

ALTER TABLE user_roles
    ADD CONSTRAINT user_roles_user_id
        FOREIGN KEY (user_id)
            REFERENCES users (id);

ALTER TABLE user_roles
    ADD CONSTRAINT user_roles_role_id
        FOREIGN KEY (role_id)
            REFERENCES roles (id);