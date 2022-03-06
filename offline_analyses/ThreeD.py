import numpy as np
import math
import matplotlib.pyplot as plt


class Vector3:
    def __init__(self, x, y, z):
        self.x = x
        self.y = y
        self.z = z


def rotate_x(theta):
    return np.matrix([[1, 0, 0],
                      [0, math.cos(theta), -math.sin(theta)],
                      [0, math.sin(theta), math.cos(theta)]])


def rotate_y(theta):
    return np.matrix([[math.cos(theta), 0, math.sin(theta)],
                      [0, 1, 0],
                      [-math.sin(theta), 0, math.cos(theta)]])


def rotate_z(theta):
    return np.matrix([[math.cos(theta), -math.sin(theta), 0],
                      [math.sin(theta), math.cos(theta), 0],
                      [0, 0, 1]])


def set_axes_equal(ax: plt.Axes):
    """Set 3D plot axes to equal scale.

    Make axes of 3D plot have equal scale so that spheres appear as
    spheres and cubes as cubes.  Required since `ax.axis('equal')`
    and `ax.set_aspect('equal')` don't work on 3D.
    """

    def set_axes_radius(ax, origin, radius):
        x, y, z = origin
        ax.set_xlim3d([x - radius, x + radius])
        ax.set_ylim3d([y - radius, y + radius])
        ax.set_zlim3d([z - radius, z + radius])

    limits = np.array([
        ax.get_xlim3d(),
        ax.get_ylim3d(),
        ax.get_zlim3d(),
    ])

    origin = np.mean(limits, axis=1)
    radius = 0.5 * np.max(np.abs(limits[:, 1] - limits[:, 0]))
    set_axes_radius(ax, origin, radius)


def rotation_test(vertices):
    vertices_original = vertices.loc[:, ['x', 'z', 'y']].to_numpy()

    ax = plt.axes(projection='3d')

    ax.scatter3D(vertices_original[:, 0], vertices_original[:, 1], vertices_original[:, 2], facecolor='black', s=10)

    for idx, item in enumerate(vertices_original):
        ax.text(item[0], item[1], item[2], idx.__str__())

    rotation = [(math.pi / 2, 'red'), (math.pi, 'green'), (math.pi * 3 / 2, 'blue')]

    for r in rotation:
        vertices_rotated = np.array(vertices_original * rotate_z(r[0]))
        ax.scatter3D(vertices_rotated[:, 0], vertices_rotated[:, 1], vertices_rotated[:, 2], facecolor=r[1], s=10)

        for idx, item in enumerate(vertices_rotated):
            ax.text(item[0], item[1], item[2], idx.__str__())

    set_axes_equal(ax)


## plots


def perform_scatter(x, y, z, origin, vertex_color, face_color):

    h = plt.figure()
    ax = plt.axes(projection='3d')
    ax.scatter3D(x, y, z, facecolor=vertex_color)
    ax.scatter3D(origin['position.x'], origin['position.z'], origin['position.y'], facecolor=face_color)

    max_range = np.array([x.max() - x.min(), y.max() - y.min(), z.max() - z.min()]).max() / 2.0

    mid_x = (x.max() + x.min()) * 0.5
    mid_y = (y.max() + y.min()) * 0.5
    mid_z = (z.max() + z.min()) * 0.5
    ax.set_xlim(mid_x - max_range, mid_x + max_range)
    ax.set_ylim(mid_y - max_range, mid_y + max_range)
    ax.set_zlim(mid_z - max_range, mid_z + max_range)

    plt.xlabel("x")
    plt.ylabel("z")

    plt.show()