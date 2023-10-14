#!/usr/bin/env python3

import sys

ARMVersionToPackage = {
    "8.0": "fex-emu-armv8.0",
    "8.1": "fex-emu-armv8.0",
    "8.2": "fex-emu-armv8.2",
    "8.3": "fex-emu-armv8.2",
    "8.4": "fex-emu-armv8.4",
}

if __name__ == "__main__":
    arm_version = sys.argv[1]
    if arm_version not in ARMVersionToPackage:
        print("Unsupported ARM version: {}".format(arm_version))
        print("Supported versions: {}".format(", ".join(ARMVersionToPackage.keys())))
        sys.exit(1)
    print(ARMVersionToPackage[arm_version])
