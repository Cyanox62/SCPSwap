# SCPSwap

Allows SCP players to request to swap roles with one another at the beginning of the round if both players agree.

# Installation

**[EXILED](https://github.com/galaxy119/EXILED) must be installed for this to work.**

Place the "SCPswap.dll" file in your Plugins folder.

| Config        | Type | Default | Description
| :-------------: | :---------: | :---------: | :------ |
| swap_blacklist | Integer List | 10 | The role IDs of SCPs that are not allowed to be swapped. |
| swap_allow_new_scps | Boolean | False | Should players be allowed to swap to roles that are not spawned in. If a user requests to swap with a role that didn't spawn in this round it will just change the player to the specified role. |
| swap_timeout | Float | 60 | The amount of time from the start of the round until swapping is no longer allowed. |
| swap_request_timeout | Float | 20 | The amount of time before a swap request to another player times out. |
