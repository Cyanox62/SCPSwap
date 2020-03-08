# SCPSwap

Allows SCP players to request to swap roles with one another at the beginning of the round if both players agree.

# Installation

**[EXILED](https://github.com/galaxy119/EXILED) must be installed for this to work.**

Place the "SCPswap.dll" file in your Plugins folder.

# Usage
SCPs can type `.scpswap scpnumberhere` to request to swap with whatever player is playing the specified role. If that player accepts, your roles will be swapped.
Examples: `.scpswap 173`, `.scpswap peanut`

*Note: As shown above, common aliases for SCPs can be used in place of numbers. A full list of these aliases ican be found [here](https://github.com/Cyanox62/SCPSwap/wiki/SCP-Role-IDs).*

# Configs

| Config        | Type | Default | Description
| :-------------: | :---------: | :---------: | :------ |
| swap_blacklist | Integer List | 10 | The role IDs of SCPs that are not allowed to be swapped. A list of Role IDs can be found [here.](https://github.com/Cyanox62/SCPSwap/wiki/SCP-Role-IDs) |
| swap_allow_new_scps | Boolean | False | Should players be allowed to swap to roles that are not spawned in. If a user requests to swap with a role that didn't spawn in this round it will just change the player to the specified role. |
| swap_timeout | Float | 60 | The amount of time from the start of the round until swapping is no longer allowed. |
| swap_request_timeout | Float | 20 | The amount of time before a swap request to another player times out. |
