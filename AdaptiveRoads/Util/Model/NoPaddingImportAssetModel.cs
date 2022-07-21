namespace AdaptiveRoads.Util.Model {
    using ColossalFramework.Threading;
    using System.Collections;
    using UnityEngine;

    // ImportAssetLodded only creates lod padding for internal global:ImportAssetModel
    internal class NoPaddingImportAssetModel : ImportAssetLodded {
        private Shader m_TemplateShader;

        protected override AssetImporterTextureLoader.ResultType[] textureTypes {
            get {
                if (m_TemplateShader.name == "Custom/Net/Electricity") {
                    return new AssetImporterTextureLoader.ResultType[0]; //empty
                }
                return new AssetImporterTextureLoader.ResultType[3] {
                    AssetImporterTextureLoader.ResultType.RGB,
                    AssetImporterTextureLoader.ResultType.XYS,
                    AssetImporterTextureLoader.ResultType.APR
                };
            }
        }

        protected override int textureAnisoLevel => 8;

        public override Mesh mesh => m_Object.GetComponent<MeshFilter>().sharedMesh;

        public override Material material {
            get => m_Object.GetComponent<MeshRenderer>().sharedMaterial;
            set => m_Object.GetComponent<MeshRenderer>().sharedMaterial = value;
        }

        public override Mesh lodMesh => m_LODObject.GetComponent<MeshFilter>().sharedMesh;

        public override Material lodMaterial => m_LODObject.GetComponent<MeshRenderer>().sharedMaterial;

        public NoPaddingImportAssetModel(Shader shader) : this(null, null, shader) { }

        public NoPaddingImportAssetModel(GameObject template, PreviewCamera camera, Shader templateShader)
                : base(template, camera) {
            m_LodTriangleTarget = 50;
            m_TemplateShader = templateShader;
        }

        public static ImportAsset ImportModel(
            string path, string modelName, string shaderName) {
            var importer = new NoPaddingImportAssetModel(Shader.Find(shaderName));
            var it = importer.ImportModelCoroutine(path, modelName);
            while (it.MoveNext()) {
                //wait
            }
            return importer;
        }

        public IEnumerator ImportModelCoroutine(string path, string modelName) {
            Import(path, modelName);

            while (!CanFinalize)
                yield return null;
            FinalizeImport();

            while (!Finalized)
                yield return null;
        }

        public override void FinalizeImport() {
            FinalizeLOD();
            if (m_Object.GetComponent<Renderer>() != null) {
                CompressTextures();
            }
            m_TaskWrapper = new MultiAsyncTaskWrapper(m_TaskNames, m_Tasks);
            LoadSaveStatus.activeTask = m_TaskWrapper;
        }

        public bool CanFinalize =>
            TextureLoadingFinished && Tasks == null;

        public bool Finalized {
            get {
                if (IsLoadingModel) return false;
                if (IsLoadingTextures || !ReadyForEditing) return false;
                if (Tasks == null /* finalization has not even started */ || Tasks.isExecuting /* finalizing */) return false;
                return true;
            }
        }

        protected override void InitializeObject() { }

        protected override void CreateInfo() { }

        public override float CalculateDefaultScale() => 1;

        protected override void CopyMaterialProperties() {
            material = new Material(m_TemplateShader);
            if (material.HasProperty("_Color")) {
                material.SetColor("_Color", Color.gray);
            }
        }

        protected override void CompressTextures() {
            Material sharedMaterial = m_Object.GetComponent<Renderer>().sharedMaterial;
            if (sharedMaterial.HasProperty("_MainTex")) {
                m_Tasks[0][0] = AssetImporterTextureLoader.CompressTexture(sharedMaterial, "_MainTex", linear: false);
            }
            if (sharedMaterial.HasProperty("_XYSMap")) {
                m_Tasks[0][1] = AssetImporterTextureLoader.CompressTexture(sharedMaterial, "_XYSMap", linear: true);
            }
            if (sharedMaterial.HasProperty("_ARPMap")) {
                m_Tasks[0][2] = AssetImporterTextureLoader.CompressTexture(sharedMaterial, "_APRMap", linear: true);
            }
        }

        protected override void LoadTextures(Task<GameObject> modelLoad) {
            m_TextureTask = AssetImporterTextureLoader.LoadTextures(
                modelLoad,
                null,
                textureTypes,
                m_Path,
                m_Filename,
                lod: false,
                textureAnisoLevel,
                generateDummy: true,
                generatePadding: false);
        }

        public override void DestroyAsset() {
            // don't destroy the loaded mesh/material because we need it.
            // here we only destroy the clone.
            if (m_OriginalMesh != null) {
                UnityEngine.Object.DestroyImmediate(m_OriginalMesh);
                m_OriginalMesh = null;
            }
            if (m_OriginalLodMesh != null) {
                UnityEngine.Object.DestroyImmediate(m_OriginalLodMesh);
                m_OriginalLodMesh = null;
            }
            if (m_Object != null) {
                UnityEngine.Object.DestroyImmediate(m_Object);
                m_Object = null;
            }
            if (m_LODObject != null) {
                UnityEngine.Object.DestroyImmediate(m_LODObject);
                m_LODObject = null;
            }
        }
    }
}