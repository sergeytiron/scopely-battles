CREATE TYPE battle_status AS ENUM ('queued', 'completed', 'failed');

CREATE TABLE battles
(
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    attacker_id integer NOT NULL,
    defender_id integer NOT NULL,
    status battle_status NOT NULL DEFAULT 'queued',
    report jsonb,
    CONSTRAINT fk_battles_attacker
        FOREIGN KEY (attacker_id)
        REFERENCES players (id),
    CONSTRAINT fk_battles_defender
        FOREIGN KEY (defender_id)
        REFERENCES players (id),
    CONSTRAINT ck_battles_distinct_players CHECK (attacker_id <> defender_id)
);

CREATE INDEX ix_battles_queued ON battles (id) WHERE status = 'queued';
CREATE INDEX ix_battles_attacker_id ON battles (attacker_id);
CREATE INDEX ix_battles_defender_id ON battles (defender_id);
