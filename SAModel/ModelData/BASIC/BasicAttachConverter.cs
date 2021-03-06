using SATools.SAModel.ModelData.Buffer;
using SATools.SAModel.ObjData;
using SATools.SAModel.Structs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace SATools.SAModel.ModelData.BASIC
{
    /// <summary>
    /// Provides buffer conversion methods for BASIC
    /// </summary>
    public static class BasicAttachConverter
    {
        /// <summary>
        /// Converts the buffer data of a model to BASIC attaches
        /// </summary>
        /// <param name="model">The tip of the model hierarchy to convert</param>
        /// <param name="optimize">Whether to optimize the data</param>
        /// <param name="ignoreWeights">Convert regardless of weight information being lost</param>
        /// <param name="forceUpdate">Still convert, even if the attaches are Basic already</param>
        public static void ConvertModelToBasic(NJObject model, bool optimize = true, bool ignoreWeights = false, bool forceUpdate = false)
        {
            if(model.Parent != null)
                throw new FormatException($"Model {model.Name} is not hierarchy root!");

            if(model.AttachFormat == AttachFormat.BASIC && !forceUpdate)
                return;

            if(model.HasWeight && !ignoreWeights)
                throw new FormatException("Model is weighted, cannot convert to basic format!");

            AttachHelper.ProcessWeightlessModel(model, (cacheAtc, ogAtc) =>
            {
                // getting the vertex information
                Vector3[] positions = new Vector3[cacheAtc.vertices.Length];
                Vector3[] normals = new Vector3[positions.Length];
                bool hasNormals = false;

                for(int i = 0; i < positions.Length; i++)
                {
                    var vtx = cacheAtc.vertices[i];
                    positions[i] = vtx.Position;
                    normals[i] = vtx.Normal;
                    if(vtx.Normal != Vector3.UnitY)
                        hasNormals = true;
                }

                if(!hasNormals)
                    normals = null;

                // putting together polygons
                Mesh[] meshes = new Mesh[cacheAtc.corners.Length];
                Material[] materials = new Material[cacheAtc.corners.Length];

                for(int i = 0; i < cacheAtc.corners.Length; i++)
                {
                    // creating the material
                    Material mat = new();
                    BufferMaterial bmat = cacheAtc.materials[i];
                    if(bmat != null)
                    {
                        mat.DiffuseColor = bmat.Diffuse;
                        mat.SpecularColor = bmat.Specular;
                        mat.Exponent = bmat.SpecularExponent;
                        mat.TextureID = bmat.TextureIndex;
                        mat.FilterMode = bmat.TextureFiltering;
                        mat.MipmapDAdjust = bmat.MipmapDistanceAdjust;
                        mat.SuperSample = bmat.AnisotropicFiltering;
                        mat.ClampU = bmat.ClampU;
                        mat.ClampV = bmat.ClampV;
                        mat.MirrorU = bmat.MirrorU;
                        mat.MirrorV = bmat.MirrorV;
                        mat.UseAlpha = bmat.UseAlpha;
                        mat.SourceAlpha = bmat.SourceBlendMode;
                        mat.DestinationAlpha = bmat.DestinationBlendmode;
                        mat.DoubleSided = !bmat.Culling;

                        mat.IgnoreLighting = bmat.HasAttribute(MaterialAttributes.noDiffuse);
                        mat.IgnoreSpecular = bmat.HasAttribute(MaterialAttributes.noSpecular);
                        mat.UseTexture = bmat.HasAttribute(MaterialAttributes.useTexture);
                        mat.EnvironmentMap = bmat.HasAttribute(MaterialAttributes.normalMapping);
                    }
                    materials[i] = mat;

                    // creating the polygons

                    BufferCorner[] bCorners = cacheAtc.corners[i];
                    IPoly[] triangles = new IPoly[bCorners.Length / 3];
                    Vector2[] texcoords = new Vector2[bCorners.Length];
                    Color[] colors = new Color[bCorners.Length];

                    Triangle current = new();
                    for(int j = 0; j < bCorners.Length; j++)
                    {
                        BufferCorner corner = bCorners[j];

                        int vIndex = j % 3;
                        current.Indices[vIndex] = corner.VertexIndex;
                        if(vIndex == 2)
                        {
                            triangles[(j-2) / 3] = current;
                            current = new Triangle();
                        }

                        texcoords[j] = corner.Texcoord;
                        colors[j] = corner.Color;
                    }

                    bool hasTexcoords = texcoords.Any(x => x != default);
                    bool hasColors = colors.Any(x => x != Color.White);

                    Mesh basicmesh = new (BASICPolyType.Triangles, triangles, false, hasColors, hasTexcoords, (ushort)i);
                    if(hasColors)
                        basicmesh.Colors = colors;
                    if(hasTexcoords)
                        basicmesh.Texcoords = texcoords;

                    meshes[i] = basicmesh;
                }

                BasicAttach result = new(positions, normals, meshes, materials);

                if(optimize)
                {

                }

                result.Name = ogAtc.Name;
                if(ogAtc is BasicAttach ogBasic)
                {
                    result.PositionName = ogBasic.PositionName;
                    result.NormalName = ogBasic.NormalName;
                    result.MaterialName = ogBasic.MaterialName;
                    result.MeshName = ogBasic.MeshName;

                    for(int j = 0; j < result.Meshes.Length; j++)
                    {
                        // lets just assume that the order of these is the same
                        Mesh mesh = result.Meshes[j];
                        Mesh ogMesh = ogBasic.Meshes[j];

                        mesh.PolyName = ogMesh.PolyName;
                        mesh.NormalName = ogMesh.NormalName;
                        mesh.ColorName = ogMesh.ColorName;
                        mesh.TexcoordName = ogMesh.TexcoordName;
                    }
                }

                return result;
            });
        }

        /// <summary>
        /// Generates Buffer meshes for all attaches in the model
        /// </summary>
        /// <param name="model">The tip of the model hierarchy to convert</param>
        /// <param name="optimize">Whether the buffer model should be optimized</param>
        public static void ConvertModelFromBasic(NJObject model, bool optimize = true)
        {
            if(model.Parent != null)
                throw new FormatException($"Model {model.Name} is not hierarchy root!");

            HashSet<BasicAttach> attaches = new();
            NJObject[] models = model.GetObjects();

            foreach(NJObject obj in models)
            {
                if(obj.Attach == null)
                    continue;
                if(obj.Attach.Format != AttachFormat.BASIC)
                    throw new FormatException("Not all Attaches inside the model are a BASIC attaches! Cannot convert");

                BasicAttach atc = (BasicAttach)obj.Attach;

                attaches.Add(atc);
            }

            foreach(BasicAttach atc in attaches)
            {
                // get the vertices
                BufferVertex[] verts = new BufferVertex[atc.Positions.Length];
                for(ushort i = 0; i < verts.Length; i++)
                    verts[i] = new BufferVertex(atc.Positions[i], atc.Normals?[i] ?? Vector3.UnitY, i);

                List<BufferMesh> meshes = new();
                foreach(Mesh mesh in atc.Meshes)
                {
                    // creating the material
                    BufferMaterial bMat;
                    if(atc.Materials != null && mesh.MaterialID < atc.Materials.Length)
                    {
                        Material mat = atc.Materials[mesh.MaterialID];
                        bMat = new BufferMaterial()
                        {
                            Diffuse = mat.DiffuseColor,
                            Specular = mat.SpecularColor,
                            SpecularExponent = mat.Exponent,
                            TextureIndex = mat.TextureID,
                            TextureFiltering = mat.FilterMode,
                            MipmapDistanceAdjust = mat.MipmapDAdjust,
                            AnisotropicFiltering = mat.SuperSample,
                            ClampU = mat.ClampU,
                            ClampV = mat.ClampV,
                            MirrorU = mat.MirrorU,
                            MirrorV = mat.MirrorV,
                            UseAlpha = mat.UseAlpha,
                            SourceBlendMode = mat.SourceAlpha,
                            DestinationBlendmode = mat.DestinationAlpha,
                            Culling = !mat.DoubleSided,
                            MaterialAttributes = MaterialAttributes.noAmbient
                        };
                        //bMat.SetAttribute(MaterialAttributes.Flat, mesh.Colors != null);
                        bMat.SetAttribute(MaterialAttributes.noDiffuse, mat.IgnoreLighting);
                        bMat.SetAttribute(MaterialAttributes.noSpecular, mat.IgnoreSpecular);
                        bMat.SetAttribute(MaterialAttributes.useTexture, mat.UseTexture);
                        bMat.SetAttribute(MaterialAttributes.normalMapping, mat.EnvironmentMap);
                    }
                    else
                    {
                        bMat = new BufferMaterial()
                        {
                            Diffuse = new Color(0xF9, 0xF9, 0xF9, 0xFF)
                        };
                    }

                    List<BufferCorner> corners = new();
                    List<uint> triangles = new();
                    int polyIndex = 0;

                    foreach(IPoly p in mesh.Polys)
                    {
                        uint l = (uint)corners.Count;
                        switch(mesh.PolyType)
                        {
                            case BASICPolyType.Triangles:
                                triangles.AddRange(new uint[] { l, l + 1, l + 2 });
                                break;
                            case BASICPolyType.Quads:
                                triangles.AddRange(new uint[] { l, l + 1, l + 2, /**/ l + 2, l + 1, l + 3 });
                                break;
                            case BASICPolyType.NPoly:
                            case BASICPolyType.Strips:
                                Strip s = (Strip)p;
                                bool rev = s.Reversed;
                                for(uint i = 2; i < s.Indices.Length; i++)
                                {
                                    uint li = l + i;
                                    if(!rev)
                                        triangles.AddRange(new uint[] { li - 2, li - 1, li });
                                    else
                                        triangles.AddRange(new uint[] { li - 1, li - 2, li });
                                    rev = !rev;
                                }
                                break;
                            default:
                                break;
                        }

                        for(int i = 0; i < p.Indices.Length; i++)
                        {
                            corners.Add(new BufferCorner(p.Indices[i], mesh.Colors?[polyIndex] ?? Color.White, mesh.Texcoords?[polyIndex] ?? Vector2.Zero));
                            polyIndex++;
                        }
                    }

                    if(meshes.Count == 0)
                        meshes.Add(new BufferMesh(verts, false, corners.ToArray(), triangles.ToArray(), bMat));
                    else
                        meshes.Add(new BufferMesh(corners.ToArray(), triangles.ToArray(), bMat));
                }

                if(optimize)
                {
                    for(int i = 0; i < meshes.Count; i++)
                        meshes[i].Optimize();
                }

                atc.MeshData = meshes.ToArray();
            }
        }
    }
}
