-- PDX Underground Game Database Schema
-- Optimized for Unity C# Integration
-- Created: 2025-04-08

-- Use DBML to define your database structure
-- Docs: https://dbml.dbdiagram.io/docs

Table users {
  id integer [primary key]
  username varchar(50) [not null]
  role varchar(50) -- Player, Admin, Moderator, etc.
  creation_date datetime [default: `now()`]
  last_login datetime
  email varchar(100)
  password_hash varchar(255)
}

Table posts {
  id int [primary key]
  title varchar(50)
  body text [note: 'Content of the post']
  user_id int [ref: > users.id]
  status varchar(50) -- draft, published, archived
  created_at datetime [default: `now()`]
  updated_at datetime
  
  note: 'Forum or bulletin board posts'
}

-- Core player data
Table player {
  id int [primary key]
  user_id int [ref: > users.id] -- Link to user account
  active_character_id int -- Current selected character
  player_lvl int [default: 1] -- Overall account level
  player_xp int [default: 0] -- Overall account experience
  creation_date datetime [default: `now()`]
  last_played datetime
  
  note: 'Player account data, separate from user authentication'
}

-- Character data
Table characters {
  id integer [primary key]
  player_id int [ref: > player.id] -- Which player owns this character
  char_name varchar(50) [not null]
  char_class varchar(30) [not null] -- Warrior, Mage, Rogue, etc.
  weapon_type varchar(30)
  armor_type varchar(30)
  level int [default: 1]
  experience int [default: 0]
  current_health int
  max_health int
  current_mana int
  max_mana int
  created_at datetime [default: `now()`]
  last_updated datetime
  
  note: 'Playable character instances'
}

-- Character base statistics
Table char_stats {
  id int [primary key]
  character_id int [ref: > characters.id]
  health int [default: 100]
  strength int [default: 10]
  vitality int [default: 10]
  agility int [default: 10]
  mana int [default: 100]
  energy int [default: 100]
  intellect int [default: 10]
  wisdom int [default: 10]
  charisma int [default: 10]
  luck int [default: 10]
  
  note: 'Base character statistics, before equipment and buffs'
}

-- Special abilities available in the game
Table special_abilities {
  id int [primary key]
  name varchar(100) [not null]
  description text
  role_specific bool -- If true, only available to specific classes
  ability_type varchar(50) -- offensive, defensive, utility, etc.
  cooldown_seconds int
  mana_cost int
  energy_cost int
  
  note: 'Special abilities that characters can learn and use'
}

-- NORMALIZED APPROACH: Separate table for stat boosts
Table stat_boosts {
  id int [primary key]
  special_ability_id int [ref: > special_abilities.id] -- Which ability provides this boost
  character_id int [ref: > characters.id] -- Which character has this boost (optional for permanent boosts)
  boost_type varchar(50) [not null] -- strength, agility, health, etc.
  boost_value int [not null] -- Numerical value of the boost
  is_percentage bool [default: false] -- Whether value is absolute or percentage
  duration_seconds int -- Duration of temporary boosts (null for permanent)
  start_time datetime -- When the boost started
  
  note: 'Stat boosts from abilities, potions, or equipment'
}

-- Character abilities junction table
Table character_abilities {
  character_id int [ref: > characters.id]
  ability_id int [ref: > special_abilities.id]
  acquired_date datetime [default: `now()`]
  ability_level int [default: 1]
  is_active bool [default: true]
  
  indexes {
    (character_id, ability_id) [pk]
  }
  
  note: 'Which abilities each character has unlocked'
}

-- Items system
Table items {
  id int [primary key]
  name varchar(100) [not null]
  description text
  item_type varchar(50) -- weapon, armor, consumable, quest, etc.
  rarity varchar(30) -- common, uncommon, rare, epic, legendary
  base_value int -- Base currency value
  required_level int [default: 1]
  stackable bool [default: false]
  max_stack_size int [default: 1]
  icon_path varchar(255) -- Path to item icon in Unity
  model_path varchar(255) -- Path to 3D model in Unity (if applicable)
  
  note: 'Base item definitions'
}

-- Weapons extend items
Table weapons {
  item_id int [primary key, ref: > items.id]
  weapon_type varchar(50) -- sword, axe, staff, bow, etc.
  damage_min int
  damage_max int
  attack_speed float
  range float
  crit_chance float [default: 0.05]
  crit_multiplier float [default: 1.5]
  
  note: 'Weapon-specific attributes'
}

-- Armor extends items
Table armor {
  item_id int [primary key, ref: > items.id]
  armor_type varchar(50) -- cloth, leather, mail, plate
  defense_value int
  damage_reduction float [default: 0]
  movement_penalty float [default: 0]
  slot varchar(30) -- head, chest, legs, feet, hands
  
  note: 'Armor-specific attributes'
}

-- Consumables extend items
Table consumables {
  item_id int [primary key, ref: > items.id]
  effect_type varchar(50) -- healing, buff, teleport, etc.
  effect_value int
  duration_seconds int
  cooldown_seconds int [default: 0]
  
  note: 'Consumable-specific attributes (potions, scrolls, food)'
}

-- Inventory system
Table inventory {
  id int [primary key]
  character_id int [ref: > characters.id]
  item_id int [ref: > items.id]
  quantity int [default: 1]
  equipped bool [default: false]
  slot_position int -- Position in inventory UI
  durability int -- Current durability if applicable
  max_durability int -- Max durability if applicable
  
  note: 'Character inventory contents'
}

-- Quest system
Table quests {
  id int [primary key]
  name varchar(100) [not null]
  description text
  quest_type varchar(50) -- main, side, daily, repeatable
  min_level int [default: 1]
  experience_reward int
  gold_reward int
  item_rewards text -- JSON array of item IDs and quantities
  prereq_quest_id int -- Quest that must be completed first
  
  note: 'Available quests in the game'
}

-- Character quest progress
Table character_quests {
  character_id int [ref: > characters.id]
  quest_id int [ref: > quests.id]
  status varchar(30) -- not_started, in_progress, completed, failed
  progress_data text -- JSON data tracking quest objectives
  started_at datetime
  completed_at datetime
  
  indexes {
    (character_id, quest_id) [pk]
  }
  
  note: 'Tracks which quests characters have started/completed'
}

-- Game world locations
Table locations {
  id int [primary key]
  name varchar(100) [not null]
  description text
  region varchar(50)
  is_dangerous bool [default: false]
  min_level int [default: 1]
  background_music varchar(255) -- Path to audio file
  ambient_sounds varchar(255) -- Path to audio file
  
  note: 'Locations in the game world'
}

-- Enemies/NPCs
Table npcs {
  id int [primary key]
  name varchar(100) [not null]
  description text
  npc_type varchar(50) -- enemy, shopkeeper, quest_giver, etc.
  level int [default: 1]
  health int
  attack_damage int
  defense int
  is_hostile bool [default: false]
  loot_table_id int -- Reference to loot table
  model_path varchar(255) -- Path to 3D model in Unity
  
  note: 'Non-player characters and enemies'
}

