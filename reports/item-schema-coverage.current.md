# CDDA Item Schema Coverage

Source: `LambdaSix/Cataclysm-DDA` at `e262adb299a7613b4aedc5f12c08fe0413c56a84`.
DTO reference: `OpenCatasharpData` at `56414851c65f10826f94ee8d76c02307800c10de`.
Scope: core `data/json/items/**/*.json` (not mods). Counts describe authored source records, not resolved game entities.

## Inventory

- JSON files: **489**
- All records in those files: **10,525**
- Records with `type: ITEM`: **10,280**
- Unique top-level ITEM field names: **228**
- Authored top-level field occurrences with matching DTO property: **140,675/140,675 (100.0%)**

This is *declared-property coverage*, not a runtime deserialization pass or deep-schema coverage. Counts exclude comment-like keys beginning with //; unknown fields remain preserved by JsonExtensionData.

## Subtype membership (overlapping)

| Declared subtype | ITEM records | Built-in handler |
|---|---:|---|
| `ARMOR` | 2,983 | yes |
| `COMESTIBLE` | 1,745 | yes |
| `TOOL` | 1,332 | yes |
| `BOOK` | 625 | yes |
| `AMMO` | 501 | yes |
| `GUN` | 387 | yes |
| `MAGAZINE` | 362 | yes |
| `GUNMOD` | 172 | yes |
| `BIONIC_ITEM` | 152 | yes |
| `SEED` | 107 | yes |
| `MILLING` | 86 | yes |
| `ARTIFACT` | 48 | yes |
| `PET_ARMOR` | 29 | yes |
| `ENGINE` | 28 | yes |
| `WHEEL` | 27 | yes |
| `BREWABLE` | 24 | yes |
| `TOOLMOD` | 8 | yes |
| `COMPOSTABLE` | 1 | yes |

## Composed subtype signatures (exclusive)

| Selected type | Records |
|---|---:|
| `ARMOR` | 2,696 |
| `GENERIC` | 2,230 |
| `COMESTIBLE` | 1,542 |
| `TOOL` | 1,050 |
| `BOOK` | 625 |
| `AMMO` | 500 |
| `GUN` | 369 |
| `MAGAZINE` | 360 |
| `TOOL+ARMOR` | 214 |
| `GUNMOD` | 153 |
| `BIONIC_ITEM` | 152 |
| `SEED+COMESTIBLE` | 85 |
| `MILLING+COMESTIBLE` | 61 |
| `TOOL+ARMOR+ARTIFACT` | 29 |
| `PET_ARMOR` | 29 |
| `ENGINE` | 28 |
| `WHEEL` | 27 |
| `BREWABLE+COMESTIBLE` | 24 |
| `SEED+MILLING+COMESTIBLE` | 22 |
| `ARMOR+GUN` | 12 |
| `ARMOR+TOOL` | 10 |
| `ARMOR+TOOL+ARTIFACT` | 10 |
| `TOOL+GUNMOD` | 10 |
| `TOOLMOD` | 8 |
| `COMESTIBLE+TOOL` | 6 |
| `GUN+GUNMOD` | 6 |
| `ARMOR+ARTIFACT` | 4 |
| `COMESTIBLE+ARMOR` | 3 |
| `MILLING` | 3 |
| `ARMOR+GUNMOD` | 3 |
| `TOOL+ARTIFACT` | 3 |
| `ARMOR+MAGAZINE` | 2 |
| `ARTIFACT` | 2 |
| `COMPOSTABLE+COMESTIBLE` | 1 |
| `COMESTIBLE+AMMO` | 1 |

## Remaining unmodelled item fields (complete inventory)

| Field | ITEM records | Observed shapes | Example record |
|---|---:|---|---|

## Selected complex fields (including already modelled)

| Field | Records | Observed shapes | Typed top-level property |
|---|---:|---|---|
| `melee_damage` | 1,403 | object (1403) | yes |
| `to_hit` | 1,071 | object (1017), number (54) | yes |
| `variants` | 567 | array (567) | yes |
| `use_action` | 1,594 | object (893), array (700), string (1) | yes |
| `modes` | 71 | array (71) | yes |
| `damage` | 170 | object (170) | yes |
| `shot_damage` | 20 | object (20) | yes |
| `ranged_damage` | 80 | object (78), array (2) | yes |
| `magazines` | 0 | - | partial/no |
| `valid_mod_locations` | 302 | array (302) | yes |
| `ammo` | 189 | array (189) | yes |
| `pocket_data` | 1,710 | array (1710) | yes |
| `armor` | 1,424 | array (1424) | yes |
| `material` | 6,024 | array (6023), object (1) | yes |
| `qualities` | 540 | array (540) | yes |

## Nested field gaps

These are descriptive counts for nested objects (e.g. armor sections and pocket_data). They are not included in the top-level percentage.

| Nested path | Occurrences without matching nested DTO property |
|---|---:|
| `to_hit.grip` | 1,017 |
| `to_hit.length` | 1,017 |
| `to_hit.surface` | 1,017 |
| `to_hit.balance` | 1,017 |
| `use_action.ammo_scale` | 111 |
| `use_action.activation_message` | 107 |
| `use_action.effect_on_conditions` | 90 |
| `use_action.need_charges_msg` | 77 |
| `use_action.need_charges` | 76 |
| `use_action.vitamins` | 67 |
| `use_action.message` | 63 |
| `use_action.description` | 54 |
| `use_action.holster_prompt` | 42 |
| `use_action.holster_msg` | 42 |
| `use_action.moves_slow` | 41 |
| `use_action.effects` | 41 |
| `use_action.monster_id` | 41 |
| `use_action.difficulty` | 41 |
| `use_action.move_cost` | 40 |
| `use_action.furn_type` | 40 |
| `use_action.need_wielding` | 38 |
| `use_action.tools_needed` | 38 |
| `pocket_data.allowed_speedloaders` | 38 |
| `use_action.skills` | 37 |
| `use_action.hostile_msg` | 36 |
| `use_action.size` | 32 |
| `use_action.friendly_msg` | 32 |
| `use_action.radius` | 30 |
| `use_action.consume` | 29 |
| `use_action.need_fire` | 28 |
| `use_action.trap` | 28 |
| `use_action.practice` | 28 |
| `use_action.done_message` | 28 |
| `use_action.terrain` | 26 |
| `damage.constant_damage_multiplier` | 26 |
| `use_action.name` | 25 |
| `use_action.base` | 23 |
| `pocket_data.ememory_max` | 19 |
| `pocket_data.id` | 17 |
| `use_action.item_action_type` | 17 |
| `use_action.materials` | 17 |
| `use_action.skill` | 17 |
| `use_action.cost_scaling` | 17 |
| `use_action.place_randomly` | 17 |
| `use_action.bleed` | 14 |
| `pocket_data.max_length` | 12 |
| `use_action.tool_quality` | 12 |
| `use_action.allow_under_player` | 12 |
| `use_action.disinfectant_power` | 11 |
| `use_action.bite` | 11 |
| `use_action.target_timer` | 10 |
| `use_action.bandages_power` | 10 |
| `use_action.transform` | 10 |
| `use_action.activate` | 10 |
| `use_action.need_sunlight` | 8 |
| `use_action.verb` | 8 |
| `use_action.gerund` | 8 |
| `use_action.on_terrain` | 8 |
| `use_action.material_restricted` | 8 |
| `use_action.need_worn` | 7 |
| `use_action.use_message` | 6 |
| `use_action.noise_message` | 6 |
| `use_action.noise_id` | 6 |
| `use_action.noise_variant` | 6 |
| `use_action.noise` | 6 |
| `use_action.stat_adjustments` | 6 |
| `use_action.fields_produced` | 6 |
| `use_action.charges_needed` | 6 |
| `use_action.used_up_item` | 5 |
| `use_action.is_pet` | 5 |
| `use_action.targets` | 5 |
| `use_action.qualities_needed` | 4 |
| `use_action.need_empty` | 4 |
| `use_action.belt` | 4 |
| `use_action.spell_id` | 4 |
| `use_action.no_fail` | 4 |
| `use_action.level` | 4 |
| `use_action.broken_type` | 4 |
| `use_action.wall` | 4 |
| `use_action.floor` | 4 |
| `use_action.door_opened` | 4 |
| `use_action.door_closed` | 4 |
| `addiction_type.addiction` | 3 |
| `addiction_type.potential` | 3 |
| `pocket_data.forbidden` | 3 |
| `use_action.max_weight` | 3 |
| `use_action.transform_age` | 3 |
| `use_action.not_ready_msg` | 3 |
| `use_action.can_extend` | 3 |
| `pocket_data.material_restriction` | 3 |
| `use_action.allow_underwater` | 3 |
| `use_action.target_charges` | 2 |
| `use_action.efficiency` | 2 |
| `use_action.explosion` | 2 |
| `use_action.floor_center` | 2 |
| `use_action.bury_question` | 2 |
| `use_action.bury` | 2 |
| `use_action.scent_typeid` | 1 |
| `use_action.duration` | 1 |
| `use_action.charges_to_use` | 1 |
| `consumption_effect_on_conditions.id` | 1 |
| `consumption_effect_on_conditions.condition` | 1 |
| `consumption_effect_on_conditions.effect` | 1 |
| `pocket_data.insulation` | 1 |
| `material.paper` | 1 |
| `material.plastic` | 1 |
| `use_action.mundane` | 1 |
| `pocket_data.min_contains_volume` | 1 |
| `use_action.set_timer` | 1 |
| `use_action.target_ammo` | 1 |
| `use_action.damage_failure_msg` | 1 |
| `use_action.needs_solid_neighbor` | 1 |

## Other discriminators in the item directory

| Type | Records |
|---|---:|
| `ammunition_type` | 158 |
| `MIGRATION` | 45 |
| `effect_on_condition` | 12 |
| `effect_type` | 9 |
| `recipe` | 9 |
| `SPELL` | 6 |
| `item_group` | 3 |
| `ter_furn_transform` | 2 |
| `mutation` | 1 |

## Interpretation and limitations

- Unknown source fields are preserved, but are not typed until a DTO/converter is introduced.
- Declared subtypes count for every membership; the parser attaches all supported subtype components.
- A C# property name match is not proof that every observed JSON value shape deserializes.
- Inheritance directives and patch operations are inventoried but not applied.
- Re-run against the same fork SHA before and after DTO changes to obtain comparable coverage.
