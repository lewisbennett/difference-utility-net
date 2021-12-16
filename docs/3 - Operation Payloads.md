# Operation Payloads

Each modification operation is an `int` which contains the necessary information to process the operation.

## Creating the Operation Payload

For remove/insert operations, the operation payload is created by first taking the `X`/`Y` coordinate and bit-shifting it to the left by `4` (`coordinate << 4`). Diagonal operations do not require any encoded coordinates therefore, bit-shifting is not required. In all cases, the first 4 bits are used for storing flags that describe the operation:

* `Bit 1`: Will be `1` if the operation is an insert operation. Cannot be `1` if `bit 2` is also `1`.
* `Bit 2`: Will be `1` if the operation is a remove operation. Cannot be `1` if `bit 1` is also `1`.
* `Bit 3`: Will be `1` if the operation is part of a move operation. Will always appear in conjunction with `bit 1` or `bit 2`, and only if move detection is enabled. See [Detecting Moves](4%20-%20Detecting%20Moves.md)
* `Bit 4`: Will be `1` if the content of the item has changed and requires updating (for example: the name field of a user object).

### Examples

`17`/`00010001`: An insert operation at index `1`.

`81`/`01010010`: A remove operation at index `5`.

`8`/`00001000`: An update operation.

`0`/`00000000`: No operation required.
