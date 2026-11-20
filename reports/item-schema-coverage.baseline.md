# CDDA Item Schema Coverage

Source: `LambdaSix/Cataclysm-DDA` at `e262adb299a7613b4aedc5f12c08fe0413c56a84`.
DTO reference: `OpenCatasharpData` at `b57bc901597192669af0f005ced64b4394a5814e`.
Scope: core `data/json/items/**/*.json` (not mods). Counts describe authored source records, not resolved game entities.

## Inventory

- JSON files: **489**
- All records in those files: **10,525**
- Records with `type: ITEM`: **10,280**
- Unique top-level ITEM field names: **228**
- Authored top-level field occurrences with matching DTO property: **120,081/140,675 (85.4%)**

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
| `looks_like` | 3,100 | string (3100) | `ammo/10mm.json:matchhead_10mm_fmj` |
| `longest_side` | 1,881 | string (1881) | `ammo/10mm.json:10mm_fmj` |
| `use_action` | 1,594 | object (893), array (700), string (1) | `ammo/sling-ready_grenade.json:sling-ready_molotov` |
| `melee_damage` | 1,403 | object (1403) | `ammo/atgm.json:atgm_heat` |
| `to_hit` | 1,071 | object (1017), number (54) | `ammo/sling-ready_grenade.json:sling-ready_molotov` |
| `variants` | 567 | array (567) | `ammo/60mm.json:60mm_shell_m720a1` |
| `display_type` | 540 | string (540) | `ammo.json:duct_tape` |
| `environmental_protection` | 521 | number (521) | `armor/arms_armor.json:carpet_armguards` |
| `replace_materials` | 460 | object (460) | `armor/arms_armor.json:chainmail_junk_arms` |
| `variant_type` | 424 | string (424) | `ammo/60mm.json:60mm_shell_m720a1` |
| `ascii_picture` | 419 | string (419) | `ammo/10mm.json:10mm_fmj` |
| `faults` | 415 | array (415) | `armor/head_attachments.json:military_nvg` |
| `phase` | 338 | string (338) | `ammo/chemical_spray.json:gas_chloramine` |
| `valid_mod_locations` | 302 | array (302) | `classes/gun.json:launcher_base` |
| `techniques` | 294 | array (294) | `armor/arms_armor.json:2byarm_guard` |
| `barrel_length` | 294 | string (294) | `gun/10mm.json:glock_29` |
| `charges` | 288 | number (288) | `chemicals_and_resources.json:bleach` |
| `weapon_category` | 271 | array (271) | `classes/gun.json:gun_base_handgun_manual` |
| `tool_ammo` | 270 | array (201), string (69) | `armor/bespoke_armor/custom_bodysuits.json:armor_nomad_advanced` |
| `read_skill` | 197 | string (197) | `book/abstract.json:book_nonf_hard_cook_tpl` |
| `revert_to` | 178 | string (178) | `armor/bespoke_armor/custom_bodysuits.json:armor_nomad_advanced_on` |
| `healthy` | 178 | number (178) | `biosignatures.json:feces_bird` |
| `damage` | 170 | object (170) | `ammo/10mm.json:10mm_fmj` |
| `difficulty` | 149 | number (149) | `bionics.json:bio_adrenaline` |
| `smoking_result` | 148 | string (148) | `comestibles/carnivore.json:froglegs` |
| `material_thickness` | 146 | number (1049) | `armor/ammo_pouch.json:ammo_pouch` |
| `stim` | 143 | number (143) | `chemicals_and_resources.json:fungicide` |
| `clip_size` | 140 | number (140) | `gun/20shot.json:mossberg_500_20` |
| `effects` | 135 | array (135) | `ammo/10mm.json:10mm_fmj` |
| `addiction_potential` | 132 | number (132) | `comestibles/alcohol.json:wine_mycus` |
| `dispersion` | 120 | number (421) | `ammo/10mm.json:10mm_fmj` |
| `addiction_type` | 119 | string (114), array (5) | `comestibles/alcohol.json:wine_mycus` |
| `conditional_names` | 118 | array (118) | `armor/jewelry.json:simple_pistol_cartridge_necklace` |
| `sided` | 117 | boolean (117) | `armor/ammo_pouch.json:legpouch` |
| `freezing_point` | 116 | number (116) | `chemicals_and_resources.json:bleach` |

## Selected complex fields (including already modelled)

| Field | Records | Observed shapes | Typed top-level property |
|---|---:|---|---|
| `melee_damage` | 1,403 | object (1403) | partial/no |
| `to_hit` | 1,071 | object (1017), number (54) | partial/no |
| `variants` | 567 | array (567) | partial/no |
| `use_action` | 1,594 | object (893), array (700), string (1) | partial/no |
| `modes` | 71 | array (71) | partial/no |
| `damage` | 170 | object (170) | partial/no |
| `shot_damage` | 20 | object (20) | partial/no |
| `ranged_damage` | 80 | object (78), array (2) | partial/no |
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
| `variants.id` | 2,338 |
| `variants.description` | 2,293 |
| `variants.name` | 2,266 |
| `pocket_data.moves` | 1,326 |
| `melee_damage.bash` | 1,307 |
| `use_action.type` | 1,271 |
| `variants.weight` | 1,087 |
| `pocket_data.max_item_length` | 1,079 |
| `to_hit.grip` | 1,017 |
| `to_hit.length` | 1,017 |
| `to_hit.surface` | 1,017 |
| `to_hit.balance` | 1,017 |
| `variants.append` | 897 |
| `use_action.menu_text` | 502 |
| `use_action.target` | 498 |
| `variants.color` | 498 |
| `use_action.msg` | 494 |
| `pocket_data.description` | 478 |
| `pocket_data.flag_restriction` | 470 |
| `pocket_data.volume_encumber_modifier` | 354 |
| `armor.layers` | 298 |
| `pocket_data.holster` | 251 |
| `pocket_data.item_restriction` | 234 |
| `melee_damage.cut` | 232 |
| `pocket_data.magazine_well` | 198 |
| `pocket_data.transparent` | 186 |
| `pocket_data.ablative` | 185 |
| `damage.damage_type` | 170 |
| `use_action.moves` | 170 |
| `armor.cover_vitals` | 161 |

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

