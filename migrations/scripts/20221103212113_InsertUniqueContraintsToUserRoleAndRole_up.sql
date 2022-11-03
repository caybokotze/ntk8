ALTER TABLE roles ADD CONSTRAINT unique_role_name UNIQUE (role_name);
ALTER TABLE user_roles ADD CONSTRAINT unique_user_role UNIQUE (user_id, role_id);