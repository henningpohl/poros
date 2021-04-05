import numpy as np
import trimesh

sphere = trimesh.creation.icosphere(radius=1.0, subdivisions=4)
#sphere = trimesh.creation.uv_sphere(radius=1.0)

#for x in sphere.vertices:
#	print(x)
#	break

# https://en.wikipedia.org/wiki/Spherical_coordinate_system#Cartesian_coordinates
def pos_to_uv(pos):
	if(pos[0] == 0.0):
		phi = 0.0
	else:
		phi = (np.arctan(pos[1] / pos[0]) + 0.5 * np.pi) / np.pi
	theta = np.arccos(pos[2]) / np.pi
	return [phi, theta]

material = trimesh.visual.material.SimpleMaterial()
uvs = [pos_to_uv(v) for v in sphere.vertices]
visual = trimesh.visual.TextureVisuals(material=material, uv=uvs)
sphere.visual = visual

#print(np.min(uvs, 0))
#print(np.max(uvs, 0))

#sphere.show()

#sphere.export('NiceSphere.glb')
sphere.export('NiceSphere.dae')