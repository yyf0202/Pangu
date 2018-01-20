using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace Hotfire
{
    class ExportsPhysic : Editor
    {
        static void WriteRot(StreamWriter sw, Quaternion rot)
        {
            WriteVector4(sw, new Vector4(rot.x, rot.y, rot.z, rot.w), "rot");
        }

        static void WriteRot(BinaryWriter sw, Quaternion rot)
        {
            WriteVector4(sw, new Vector4(rot.x, rot.y, rot.z, rot.w));
        }

        static void WriteVector4(BinaryWriter sw, Vector4 vec)
        {
            sw.Write(vec.x);
            sw.Write(vec.y);
            sw.Write(vec.z);
            sw.Write(vec.w);
        }

        static void WriteVector3(BinaryWriter sw, Vector3 vec)
        {
            sw.Write(vec.x);
            sw.Write(vec.y);
            sw.Write(vec.z);
        }

        static void WriteVector4(StreamWriter sw, Vector4 vec, string title)
        {
            string rotinfo = title;
            rotinfo += ":";
            rotinfo += vec.x;
            rotinfo += ",";
            rotinfo += vec.y;
            rotinfo += ",";
            rotinfo += vec.z;
            rotinfo += ",";
            rotinfo += vec.w;
            sw.WriteLine(rotinfo);
        }

        static void WriteVector3(StreamWriter sw, Vector3 vec, string title)
        {
            string posinfo = title;
            posinfo += ":";
            posinfo += vec.x;
            posinfo += ",";
            posinfo += vec.y;
            posinfo += ",";
            posinfo += vec.z;
            sw.WriteLine(posinfo);
        }

        [MenuItem("ExportPhsic/ReplaceDiffuseShader")]
        static void ReplaceDiffuseShader()
        {
            MeshRenderer[] allColl = Selection.activeGameObject.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < allColl.Length; ++i)
            {
                for (int j = 0; j < allColl[i].materials.Length; ++j)
                {
                    allColl[i].materials[j].shader = Shader.Find("Mobile/Diffuse");
                }
            }
        }

        [MenuItem("ExportPhsic/Caulculate Relative")]
        static void CaulculateRelative()
        {
            GameObject player = Selection.activeGameObject;
            Transform hand = player.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/Hand_Gun_Hold");
            hand = hand.GetChild(0);
            Transform Clavicle = player.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 L Clavicle");
            if (hand && Clavicle)
            {
                Quaternion handrot =  hand.rotation;
                Quaternion invHandRot =  Quaternion.Inverse(handrot);
                Vector3 localPos = invHandRot * (Clavicle.position - hand.position);
                Debug.Log("localPos: " + localPos.x + " " + localPos.y + " " + localPos.z);
                Quaternion localRot = invHandRot * Clavicle.rotation;
                Debug.Log("localRot: " + localRot.x + " " + localRot.y + " " + localRot.z + " " + localRot.w);
                Debug.Log("localScale: " + Clavicle.localScale.x + " " + Clavicle.localScale.y + " " + Clavicle.localScale.z);
            }
        }

        [MenuItem("ExportPhsic/Collision(txt)")]
        static void ExportCollision()
        {
            Collider[] allColl= GameObject.FindObjectsOfType<Collider>();
            int boxColliderCount = 0;
            int meshColliderCount = 0;
            int shpereColliderCount = 0;
            int capsuleColliderCount = 0;
            string fileName = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
            string collisionFile = Application.dataPath + "/" + fileName + ".coll";
            FileStream  fstream = File.Open(collisionFile, FileMode.Create);
            StreamWriter sw = new StreamWriter(fstream);
            sw.WriteLine("coll0");
            for (int i = 0; i < allColl.Length; ++i)
            {
                Object curColl = allColl[i];
                if (curColl != null)
                {
                     System.Type curType = curColl.GetType();
                     if (curType.Equals(typeof(BoxCollider)))
                     {
                         BoxCollider box = curColl as BoxCollider;
                         if (box.gameObject.name == "Collider")
                         {
                             //Debug.Log("Test");
                             //continue;
                         }
                        if (box.gameObject.layer == 23)
                            Debug.Log("this is a speedvault wall") ;
                         sw.WriteLine("Box " + (box.gameObject.name.Length > 0 ? box.gameObject.name : "object") + " " + box.gameObject.layer);
                         WriteRot(sw, box.gameObject.transform.rotation);
                         Vector3 center = box.center;
                         //center.Scale(box.gameObject.transform.lossyScale);
                         Vector3 worldCenter = box.transform.TransformPoint(center);//box.gameObject.transform.rotation * center + box.gameObject.transform.position;
                         WriteVector3(sw, worldCenter, "pos");
                         Vector3 scale = box.gameObject.transform.lossyScale;
                         WriteVector3(sw, new Vector3(scale.x * box.size.x, scale.y * box.size.y, scale.z * box.size.z), "size");
                         boxColliderCount++;
                     }
                     else if (curType.Equals(typeof(MeshCollider)))
                     {
                         MeshCollider mesh = curColl as MeshCollider;
                         sw.WriteLine("Mesh " + (mesh.gameObject.name.Length > 0 ? mesh.gameObject.name : "object") + " " + mesh.gameObject.layer);
                         WriteRot(sw, mesh.gameObject.transform.rotation);
                         WriteVector3(sw, mesh.gameObject.transform.position, "pos");
                         Mesh meshData = mesh.sharedMesh;
                         Vector3 globalScale = mesh.gameObject.transform.lossyScale;
                         sw.WriteLine("Vertex " + meshData.vertexCount);
                         for (int index = 0; index < meshData.vertexCount; ++index)
                         {
                             Vector3 scaledVertex = new Vector3(globalScale.x * meshData.vertices[index].x,globalScale.y * meshData.vertices[index].y,
                                                              globalScale.z * meshData.vertices[index].z);
                             WriteVector3(sw, scaledVertex, "v");
                         }

                         int totalTriangles = 0;
                         for (int index = 0; index < meshData.subMeshCount; ++index)
                         {
                             int[] indices = meshData.GetTriangles(index);
                             totalTriangles += (indices.Length / 3); 
                         }
                         sw.WriteLine("Face " + totalTriangles);
                         for (int index = 0; index < meshData.subMeshCount; ++index)
                         {
                             int[] indices = meshData.GetTriangles(index);
                             for (int k = 0; k < indices.Length / 3; k++)
                             {
                                 WriteVector3(sw, new Vector3(indices[3 * k],indices[3 * k+1],indices[3 * k+2]), "f");
                             }
                         }
                         meshColliderCount++;
                     }
                     else if (curType.Equals(typeof(SphereCollider)))
                     {
                         shpereColliderCount++;
                     }
                     else if (curType.Equals(typeof(CapsuleCollider)))
                     {
                         capsuleColliderCount++;
                     }
                }
            }
            sw.Close();
            fstream.Close();
            Debug.Log("collisionFile is " + collisionFile);
            Debug.Log("boxColliderCount: " + boxColliderCount + " meshColliderCount: " + meshColliderCount);
            Debug.Log("shpereColliderCount: " + shpereColliderCount + " capsuleColliderCount: " + capsuleColliderCount);
            Debug.Log(EditorApplication.currentScene);
            Debug.Log(Application.dataPath);
        }

        static void WriteObjVertex(StreamWriter outStream,Vector3 vertex)
        {
            outStream.Write("v" + " " + vertex.x + " " + vertex.y + " " + vertex.z + "\n");
        }

        static void WriteObjFace(StreamWriter outStream, int x, int y, int z)
        {
            outStream.Write("f" + " " + x + " " + y + " " + z + "\n");
        }

        static void WriteObjFace(StreamWriter outStream, int x, int y, int z, int w)
        {
            outStream.Write("f" + " " + x + " " + y + " " + z + " " + w + "\n" );
        }


        [MenuItem("ExportPhsic/Export OBJ for NavMesh")]
        static void ExportOBJ()
        {
            Collider[] allColl = GameObject.FindObjectsOfType<Collider>();
            int boxColliderCount = 0;
            int meshColliderCount = 0;
            int shpereColliderCount = 0;
            int capsuleColliderCount = 0;
            int totalVertexCount = 1;
            string fileName = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
            string collisionFile = Application.dataPath + "/" + fileName + ".obj";
            FileStream fstream = File.Open(collisionFile, FileMode.Create);
            StreamWriter sw = new StreamWriter(fstream, Encoding.ASCII);
            sw.Write("mtllib nav_test.mtl\n");
            float total = allColl.Length;
            for (int i = 0; i < allColl.Length; ++i)
            {
                Object curColl = allColl[i];
                EditorUtility.DisplayProgressBar("Export OBJ", "Export OBJ......", i / total);
                if (curColl != null)
                {
                    System.Type curType = curColl.GetType();
                    if (curType.Equals(typeof(BoxCollider)))
                    {
                        BoxCollider box = curColl as BoxCollider;
                        if (box.gameObject.name == "Collider")
                        {
                            //Debug.Log("Test");
                            //continue;
                        }
                        Vector3 center = box.center;
                        //center.Scale(box.gameObject.transform.lossyScale);
                        Vector3 worldCenter = box.transform.TransformPoint(center);
                        Vector3 scale = box.gameObject.transform.lossyScale;
                        Vector3[] vertexs = new Vector3[8];
                        vertexs[0] = new Vector3(-0.5f, 0.5f, -0.5f);
                        vertexs[1] = new Vector3(-0.5f, 0.5f, 0.5f);
                        vertexs[2] = new Vector3(0.5f, 0.5f, 0.5f);
                        vertexs[3] = new Vector3(0.5f, 0.5f, -0.5f);

                        vertexs[4] = new Vector3(-0.5f, -0.5f, -0.5f);
                        vertexs[5] = new Vector3(-0.5f, -0.5f, 0.5f);
                        vertexs[6] = new Vector3(0.5f, -0.5f, 0.5f);
                        vertexs[7] = new Vector3(0.5f, -0.5f, -0.5f);

                        Matrix4x4 matFinal = Matrix4x4.TRS(worldCenter, box.gameObject.transform.rotation,new Vector3(scale.x * box.size.x, scale.y * box.size.y, scale.z * box.size.z));
                        sw.Write("o" + " " + box.gameObject.name + "\n");
                        for (int j = 0; j < vertexs.Length; ++j )
                        {
                            vertexs[j] = matFinal.MultiplyPoint(vertexs[j]);
                            WriteObjVertex(sw,vertexs[j]);
                        }
                        sw.Write("g" + " " + "Generic" + "\n");
                        sw.Write("usemtl Default\n");
                        totalVertexCount -= 1;
                        //WriteObjFace(sw, 1 + totalVertexCount, 4 + totalVertexCount, 8 + totalVertexCount, 5 + totalVertexCount);
                        WriteObjFace(sw, 1 + totalVertexCount, 4 + totalVertexCount, 8 + totalVertexCount);
                        WriteObjFace(sw, 1 + totalVertexCount, 8 + totalVertexCount, 5 + totalVertexCount);
                        //WriteObjFace(sw, 1 + totalVertexCount, 2 + totalVertexCount, 3 + totalVertexCount, 4 + totalVertexCount);
                        WriteObjFace(sw, 1 + totalVertexCount, 2 + totalVertexCount, 3 + totalVertexCount);
                        WriteObjFace(sw, 1 + totalVertexCount, 3 + totalVertexCount, 4 + totalVertexCount);
                        //WriteObjFace(sw, 5 + totalVertexCount, 8 + totalVertexCount, 7 + totalVertexCount, 6 + totalVertexCount);
                        WriteObjFace(sw, 5 + totalVertexCount, 8 + totalVertexCount, 7 + totalVertexCount);
                        WriteObjFace(sw, 5 + totalVertexCount, 7 + totalVertexCount, 6 + totalVertexCount);
                        //WriteObjFace(sw, 2 + totalVertexCount, 6 + totalVertexCount, 7 + totalVertexCount, 3 + totalVertexCount);
                        WriteObjFace(sw, 2 + totalVertexCount, 6 + totalVertexCount, 7 + totalVertexCount);
                        WriteObjFace(sw, 2 + totalVertexCount, 7 + totalVertexCount, 3 + totalVertexCount);
                        //WriteObjFace(sw, 4 + totalVertexCount, 3 + totalVertexCount, 7 + totalVertexCount, 8 + totalVertexCount);
                        WriteObjFace(sw, 4 + totalVertexCount, 3 + totalVertexCount, 7 + totalVertexCount);
                        WriteObjFace(sw, 4 + totalVertexCount, 7 + totalVertexCount, 8 + totalVertexCount);
                        //WriteObjFace(sw, 1 + totalVertexCount, 5 + totalVertexCount, 6 + totalVertexCount, 2 + totalVertexCount);
                        WriteObjFace(sw, 1 + totalVertexCount, 5 + totalVertexCount, 6 + totalVertexCount);
                        WriteObjFace(sw, 1 + totalVertexCount, 6 + totalVertexCount, 2 + totalVertexCount);
                        totalVertexCount += 1;
                        totalVertexCount += 8;
                        boxColliderCount++;
                    }
                    else if (curType.Equals(typeof(MeshCollider)))
                    {
                        MeshCollider mesh = curColl as MeshCollider;
                        Mesh meshData = mesh.sharedMesh;
                        if (meshData == null)
                            continue;
                        for (int a = 0; a < meshData.subMeshCount; ++a)
                        {
                            MeshTopology top = meshData.GetTopology(a);
                            if (top != MeshTopology.Triangles)
                                Debug.Log(top);
                        }
                        Vector3 globalScale = mesh.gameObject.transform.lossyScale;
                        bool bClockWise = true;
                        if (globalScale.x < 0.0)
                            bClockWise = false;
                        sw.Write("o" + " " + mesh.gameObject.name + "\n");
                        for (int index = 0; index < meshData.vertexCount; ++index)
                        {
                            Vector3 scaledVertex = new Vector3(meshData.vertices[index].x, meshData.vertices[index].y,
                                                             meshData.vertices[index].z);
                            scaledVertex = mesh.gameObject.transform.localToWorldMatrix.MultiplyPoint(scaledVertex);
                            WriteObjVertex(sw, scaledVertex);
                        }

                        int totalTriangles = 0;
                        for (int index = 0; index < meshData.subMeshCount; ++index)
                        {
                            int[] indices = meshData.GetTriangles(index);
                            totalTriangles += (indices.Length / 3);
                        }
                        sw.Write("g" + " " + "Generic" + "\n");
                        sw.Write("usemtl Default\n");
                        for (int index = 0; index < meshData.subMeshCount; ++index)
                        {
                            int[] indices = meshData.GetTriangles(index);
                            for (int k = 0; k < indices.Length / 3; k++)
                            {
                                if (bClockWise)
                                {
                                    WriteObjFace(sw, indices[3 * k] + totalVertexCount, indices[3 * k + 1] + totalVertexCount, indices[3 * k + 2] + totalVertexCount);
                                }
                                else
                                {
                                    WriteObjFace(sw, indices[3 * k] + totalVertexCount, indices[3 * k + 2] + totalVertexCount, indices[3 * k + 1] + totalVertexCount);
                                }
               
                            }
                        }
                        totalVertexCount += meshData.vertexCount;
                        meshColliderCount++;
                    }
                    else if (curType.Equals(typeof(SphereCollider)))
                    {
                        shpereColliderCount++;
                    }
                    else if (curType.Equals(typeof(CapsuleCollider)))
                    {
                        capsuleColliderCount++;
                    }
                }
            }
            sw.Close();
            fstream.Close();
            EditorUtility.ClearProgressBar();
            string moveToDir = Application.dataPath + "/" + "../../" + fileName + ".obj";
            if (File.Exists(moveToDir))
                File.Delete(moveToDir);
            File.Move(collisionFile, moveToDir);
            Debug.Log("boxColliderCount: " + boxColliderCount + " meshColliderCount: " + meshColliderCount);
            Debug.Log("shpereColliderCount: " + shpereColliderCount + " capsuleColliderCount: " + capsuleColliderCount);
            Debug.Log(EditorApplication.currentScene);
            Debug.Log(Application.dataPath);
        }

        [MenuItem("ExportPhsic/Collision(bin)")]
        static void ExportCollisionBin()
        {
            Collider[] allColl = GameObject.FindObjectsOfType<Collider>();
            int boxColliderCount = 0;
            int meshColliderCount = 0;
            int shpereColliderCount = 0;
            int capsuleColliderCount = 0;
            string fileName = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
            string collisionFile = Application.dataPath + "/" + fileName + ".coll";
            FileStream fstream = File.Open(collisionFile, FileMode.Create);
            BinaryWriter sw = new BinaryWriter(fstream, Encoding.UTF8);
            sw.Write("coll0");
            float total = allColl.Length;
            for (int i = 0; i < allColl.Length; ++i)
            {
                Object curColl = allColl[i];
                EditorUtility.DisplayProgressBar("Export Physic", "Export Collide......", i / total);
                if (curColl != null)
                {
                    System.Type curType = curColl.GetType();
                    if (curType.Equals(typeof(BoxCollider)))
                    {
                        BoxCollider box = curColl as BoxCollider;
                        if (box.gameObject.name == "Collider")
                        {
                            //Debug.Log("Test");
                            //continue;
                        }
                        sw.Write(1);
                        sw.Write((box.gameObject.name.Length > 0 ? box.gameObject.name : "object"));
                        sw.Write(box.gameObject.layer);
                        WriteRot(sw, box.gameObject.transform.rotation);
                        Vector3 center = box.center;
                        //center.Scale(box.gameObject.transform.lossyScale);
                        Vector3 worldCenter = box.transform.TransformPoint(center);//box.gameObject.transform.rotation * center + box.gameObject.transform.position;
                        WriteVector3(sw, worldCenter);
                        Vector3 scale = box.gameObject.transform.lossyScale;
                        WriteVector3(sw, new Vector3(scale.x * box.size.x, scale.y * box.size.y, scale.z * box.size.z));
                        boxColliderCount++;
                    }
                    else if (curType.Equals(typeof(MeshCollider)))
                    {
                        MeshCollider mesh = curColl as MeshCollider;
						Mesh meshData = mesh.sharedMesh;
						if(meshData == null)
							continue;
                        for (int a = 0; a < meshData.subMeshCount; ++a)
                        {
                            MeshTopology top = meshData.GetTopology(a);
                            if (top != MeshTopology.Triangles)
                                Debug.Log(top);
                        }
                        Vector3 globalScale = mesh.gameObject.transform.lossyScale;
                        sw.Write(2);
                        sw.Write((mesh.gameObject.name.Length > 0 ? mesh.gameObject.name : "object"));
                        sw.Write(mesh.gameObject.layer);
                        if (globalScale.x < 0.0)
                            sw.Write(1);
                        else
                            sw.Write(0);
                        WriteRot(sw, mesh.gameObject.transform.rotation);
                        WriteVector3(sw, mesh.gameObject.transform.position);
                        sw.Write( meshData.vertexCount);
                        for (int index = 0; index < meshData.vertexCount; ++index)
                        {
                            Vector3 scaledVertex = new Vector3(globalScale.x * meshData.vertices[index].x, globalScale.y * meshData.vertices[index].y,
                                                             globalScale.z * meshData.vertices[index].z);
                            WriteVector3(sw, scaledVertex);
                        }

                        int totalTriangles = 0;
                        for (int index = 0; index < meshData.subMeshCount; ++index)
                        {
                            int[] indices = meshData.GetTriangles(index);
                            totalTriangles += (indices.Length / 3);
                        }
                        sw.Write(totalTriangles);
                        for (int index = 0; index < meshData.subMeshCount; ++index)
                        {
                            int[] indices = meshData.GetTriangles(index);
                            for (int k = 0; k < indices.Length / 3; k++)
                            {
                                WriteVector3(sw, new Vector3(indices[3 * k], indices[3 * k + 1], indices[3 * k + 2]));
                            }
                        }
                        meshColliderCount++;
                    }
                    else if (curType.Equals(typeof(SphereCollider)))
                    {
                        shpereColliderCount++;
                    }
                    else if (curType.Equals(typeof(CapsuleCollider)))
                    {
                        capsuleColliderCount++;
                    }
                }
            }
            sw.Close();
            fstream.Close();
            EditorUtility.ClearProgressBar();
            string moveToDir = Application.dataPath + "/" + "../../Server/tps-battleServer/phys/maps/" + fileName + ".coll";
            if (File.Exists(moveToDir))
                File.Delete(moveToDir);
            File.Move(collisionFile, moveToDir);
            Debug.Log("boxColliderCount: " + boxColliderCount + " meshColliderCount: " + meshColliderCount);
            Debug.Log("shpereColliderCount: " + shpereColliderCount + " capsuleColliderCount: " + capsuleColliderCount);
            Debug.Log(EditorApplication.currentScene);
            Debug.Log(Application.dataPath);
        }

        [MenuItem("ExportPhsic/removeCollide")]
        static void RemoveCollide()
        {
            Collider[] allColl = GameObject.FindObjectsOfType<Collider>();
            for (int i = 0; i < allColl.Length; ++i)
            {
                Object.DestroyImmediate(allColl[i]);
            }
        }
    }
}
