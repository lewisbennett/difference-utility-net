# Detecting Moves

**"Moves"** describe the concept of an item that is persistant between the source and destination collections, but its position has changed. This is a concept that the diff algorithm we've used so far does not take into account thus, an extra layer had to be built. If items are arranged such that moves would never be necessary, move detection can be disabled (for example: entries ordered by date) to increase performance.
