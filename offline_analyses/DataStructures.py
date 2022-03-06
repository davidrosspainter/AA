from dataclasses import dataclass
from enum import Enum
import math


@dataclass
class State(Enum):
    start = 0
    cue = 1
    array = 2
    wait = 3
    feedback = 4


# @dataclass
# class Orientation(Enum):
#     negativeX = 0
#     positiveX = 1
#     negativeZ = 2
#     positiveZ = 3


class Orientation:
    negativeX = 0
    positiveX = 1
    negativeZ = 2
    positiveZ = 3


rotationDictionary = {Orientation.negativeX: 0,  # default orientation
                      Orientation.positiveX: math.pi,
                      Orientation.negativeZ: math.pi*1/2,
                      Orientation.positiveZ: -math.pi*1/2}


@ dataclass
class RayCastSources(Enum):
    Headset = 1
    Controller = 2
    Gaze = 3
