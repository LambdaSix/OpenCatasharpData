# CDDA Item Schema Coverage

Source: `LambdaSix/Cataclysm-DDA` at `e262adb299a7613b4aedc5f12c08fe0413c56a84`.
DTO reference: `OpenCatasharpData` at `4743e098d582607c2b52e7cec95bcf6a59af107c`.
Scope: core `data/json/items/**/*.json` (not mods). Counts describe authored source records, not resolved game entities.

## Inventory

- JSON files: **489**
- All records in those files: **10,525**
- Records with `type: ITEM`: **10,280**
- Unique top-level ITEM field names: **228**
- Authored top-level field occurrences with matching DTO property: **136,313/140,675 (96.9%)**

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
| `BIONIC_ITEM` | 152 | no |
| `SEED` | 107 | no |
| `MILLING` | 86 | no |
| `ARTIFACT` | 48 | no |
| `PET_ARMOR` | 29 | no |
| `ENGINE` | 28 | no |
| `WHEEL` | 27 | no |
| `BREWABLE` | 24 | no |
| `TOOLMOD` | 8 | no |
| `COMPOSTABLE` | 1 | no |

## Actual DTO routing (exclusive)

| Selected type | Records |
|---|---:|
| `ARMOR` | 2,737 |
| `GENERIC` | 2,479 |
| `COMESTIBLE` | 1,745 |
| `TOOL` | 1,306 |
| `BOOK` | 625 |
| `AMMO` | 500 |
| `GUN` | 375 |
| `MAGAZINE` | 360 |
| `GUNMOD` | 153 |

## Most frequent unmodelled item fields

| Field | ITEM records | Observed shapes | Example record |
|---|---:|---|---|
| `healthy` | 178 | number (178) | `biosignatures.json:feces_bird` |
| `difficulty` | 149 | number (149) | `bionics.json:bio_adrenaline` |
| `smoking_result` | 148 | string (148) | `comestibles/carnivore.json:froglegs` |
| `material_thickness` | 146 | number (1049) | `armor/ammo_pouch.json:ammo_pouch` |
| `stim` | 143 | number (143) | `chemicals_and_resources.json:fungicide` |
| `addiction_potential` | 132 | number (132) | `comestibles/alcohol.json:wine_mycus` |
| `addiction_type` | 119 | string (114), array (5) | `comestibles/alcohol.json:wine_mycus` |
| `sided` | 117 | boolean (117) | `armor/ammo_pouch.json:legpouch` |
| `freezing_point` | 116 | number (116) | `chemicals_and_resources.json:bleach` |
| `warmth` | 115 | number (954) | `armor/ammo_pouch.json:xlkevlar` |
| `plant_name` | 107 | string (107) | `comestibles/other.json:dry_beans` |
| `fruit` | 107 | string (107) | `comestibles/other.json:dry_beans` |
| `growth_stages` | 107 | array (107) | `comestibles/other.json:dry_beans` |
| `snippet_category` | 101 | string (63), array (38) | `armor/coats.json:jacket_jean` |
| `range` | 101 | number (235) | `ammo/10mm.json:10mm_fmj` |
| `power_draw` | 87 | string (87) | `armor/bespoke_armor/custom_bodysuits.json:armor_nomad_advanced_on` |
| `byproducts` | 75 | array (75) | `comestibles/other.json:dry_beans` |
| `container` | 68 | string (539) | `ammo/chemical_spray.json:gas_chloramine` |
| `into` | 66 | string (66) | `comestibles/nuts.json:pistachio_shelled` |
| `recipe` | 66 | string (66) | `comestibles/nuts.json:pistachio_shelled` |
| `proficiencies` | 63 | array (63) | `book/chemistry.json:adv_chemistry` |
| `reload_noise_volume` | 63 | number (63) | `classes/gun.json:underbarrel_base` |
| `countdown_interval` | 60 | string (60) | `armor/exotic.json:migo_plate` |
| `seeds` | 59 | boolean (59) | `comestibles/other.json:dry_beans` |
| `sight_dispersion` | 54 | number (54) | `classes/gun.json:gun_base_handgun_manual` |
| `barrel_volume` | 54 | string (54) | `gun/20shot.json:savage_model_220` |
| `integral_longest_side` | 54 | string (54) | `gun/robofac_gun.json:robofac_gun_ar` |
| `repairs_with` | 51 | array (51) | `armor/arms_armor.json:armguard_metal` |
| `light` | 50 | number (50) | `armor/bespoke_armor/custom_storage.json:nomad_rig_on` |
| `loudness` | 49 | number (91) | `ammo/20shot.json:20shot_slug` |
| `parasites` | 47 | number (47) | `comestibles/carnivore.json:flesh` |
| `recovery_chance` | 46 | number (46) | `ammo.json:rock` |
| `min_cycle_recoil` | 41 | number (41) | `classes/gun.json:gun_base_22_semi_pistol` |
| `passive_effects` | 35 | array (35) | `armor/bespoke_armor/custom_bodysuits.json:armor_nomad` |
| `add_mod` | 35 | array (35) | `gun/robofac_gun.json:robofac_gun_exodii` |

## Selected complex fields (including already modelled)

| Field | Records | Observed shapes | Typed top-level property |
|---|---:|---|---|
| `melee_damage` | 1,403 | object (1403) | yes |
| `to_hit` | 1,071 | object (1017), number (54) | yes |
| `variants` | 567 | array (567) | yes |
| `use_action` | 1,594 | object (893), array (700), string (1) | yes |
| `modes` | 71 | array (71) | partial/no |
| `damage` | 170 | object (170) | yes |
| `shot_damage` | 20 | object (20) | yes |
| `ranged_damage` | 80 | object (78), array (2) | yes |
| `magazines` | 0 | - | partial/no |
| `valid_mod_locations` | 302 | array (302) | partial/no |
| `ammo` | 189 | array (189) | partial/no |
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
| `armor.volume_encumber_modifier` | 160 |
| `pocket_data.default_magazine` | 136 |
| `armor.rigid_layer_only` | 125 |
| `use_action.ammo_scale` | 111 |
| `use_action.activation_message` | 107 |
| `use_action.effect_on_conditions` | 90 |
| `pocket_data.open_container` | 85 |
| `pocket_data.ripoff` | 80 |
| `pocket_data.min_item_length` | 78 |
| `use_action.need_charges_msg` | 77 |
| `use_action.need_charges` | 76 |
| `pocket_data.extra_encumbrance` | 70 |
| `use_action.vitamins` | 67 |
| `use_action.message` | 63 |
| `pocket_data.min_item_volume` | 61 |
| `pocket_data.max_item_volume` | 60 |
| `armor.cover_ranged` | 55 |
| `use_action.description` | 54 |
| `pocket_data.airtight` | 44 |
| `use_action.holster_prompt` | 42 |
| `use_action.holster_msg` | 42 |
| `use_action.moves_slow` | 41 |
| `use_action.effects` | 41 |
| `use_action.monster_id` | 41 |
| `use_action.difficulty` | 41 |
| `use_action.move_cost` | 40 |

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
- Combined subtypes are counted for every membership; the current parser chooses the first recognised subtype.
- A C# property name match is not proof that every observed JSON value shape deserializes.
- Inheritance directives and patch operations are inventoried but not applied.
- Re-run against the same fork SHA before and after DTO changes to obtain comparable coverage.

