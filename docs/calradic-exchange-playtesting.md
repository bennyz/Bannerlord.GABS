# Calradic Exchange playtesting

This fork supports cheat-assisted, unattended test passes of Calradic Exchange on Bannerlord 1.5.2. The goal is useful feature evidence, not a human-like campaign run.

## Test contract

Use three distinct phases:

1. **Arrange with cheats.** Load a disposable save, enable cheat mode, add money, move to a suitable settlement, and suppress hostile encounters.
2. **Act through the feature.** Open Calradic Exchange and use its Gauntlet ViewModel commands and writable fields. Do not bypass the behavior being tested with a console command.
3. **Assert with two signals.** Capture the player-visible screen and inspect the resulting ViewModel or an appropriate read-only `cx.*` diagnostic command.

Cheats are valid for setup and for removing campaign noise. They are not evidence that the tested UI workflow works.

## Recommended setup

```text
core/check_blockers
core/set_cheat_mode enabled=true
inventory/add_gold amount=1000000
party/set_encounter_protection enabled=true hours=720
core/save_game saveName=<disposable-baseline>
```

For time-dependent scenarios, check blockers, set speed to 10, use the relevant blocking awaiter, then pause before inspecting UI state:

```text
core/check_blockers
core/set_time_speed speed=10
<action-specific wait tool>
core/set_time_speed speed=0
```

Do not use wall-clock sleeps. Handle inquiries, conversations, missions, and menu interruptions explicitly.

## Calradic Exchange UI workflow

1. Use `ui/get_screen` to identify the Calradic Exchange Gauntlet layer and available commands.
2. Read the relevant property with `ui/get_viewmodel_property`.
3. Fill editable fields with `ui/set_viewmodel_property`. Typical properties include `OrderQuantity`, `ForwardQuantity`, `ForwardMaturity`, `LimitQuantity`, `LimitPrice`, and `LimitDays`.
4. Submit through `ui/call_viewmodel_method` or the visible button command.
5. Take a screenshot and reread status, balance, positions, or orders.
6. When the mod exposes a matching `cx.*` console diagnostic, use `core/run_command` as the second assertion signal.

## Safety rules

- Use a named disposable save and never overwrite the user's campaign save.
- Keep encounter protection enabled during accelerated map time. Reapply it when the requested campaign duration exceeds its remaining window.
- Pause campaign time before multi-step UI interaction.
- Stop on crashes, load failures, contradictory state, or an unknown destructive prompt. Preserve logs and the last screenshot.
- Restore normal behavior with `party/set_encounter_protection enabled=false hours=0` when the pass ends.

## Useful pass report

```text
Scenario:
Build identity: Bannerlord / Calradic Exchange / GABS commits
Save used:
Cheat setup:
Player-facing steps:
Visible result:
State/diagnostic result:
PASS | FAIL | BLOCKED:
Artifacts: screenshots and relevant log paths
Notes: unexpected behavior, ambiguity, or follow-up coverage
```

## Model guidance

A smaller agent such as Luna can run a focused pass effectively when the scenario has explicit preconditions, a bounded workflow, and concrete assertions. Give it one scenario at a time. Prefer semantic tools and ViewModel state over coordinate-based visual clicking; use screenshots as evidence and for unexpected screens.

The first prepared pass is [Luna: Gauntlet spot-order round trip](scenarios/luna-gauntlet-spot-roundtrip.md).
