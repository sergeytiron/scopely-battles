CREATE TABLE players
(
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name varchar(20) NOT NULL,
    description varchar(1000),
    gold integer NOT NULL DEFAULT 0,
    silver integer NOT NULL DEFAULT 0,
    attack_value integer NOT NULL DEFAULT 0,
    defense_value integer NOT NULL DEFAULT 0,
    hit_points integer NOT NULL,
    score bigint NOT NULL DEFAULT 0,
    CONSTRAINT uq_players_name UNIQUE (name),
    CONSTRAINT ck_players_gold_non_negative CHECK (gold >= 0),
    CONSTRAINT ck_players_gold_max CHECK (gold <= 1000000000),
    CONSTRAINT ck_players_silver_non_negative CHECK (silver >= 0),
    CONSTRAINT ck_players_silver_max CHECK (silver <= 1000000000),
    CONSTRAINT ck_players_attack_value_non_negative CHECK (attack_value >= 0),
    CONSTRAINT ck_players_defense_value_non_negative CHECK (defense_value >= 0),
    CONSTRAINT ck_players_hit_points_positive CHECK (hit_points > 0),
    CONSTRAINT ck_players_score_non_negative CHECK (score >= 0)
);

CREATE INDEX ix_players_score_desc_id ON players (score DESC, id ASC);
