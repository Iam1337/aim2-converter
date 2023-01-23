using System.IO;

namespace AimConverter
{
    public class AimModel
    {
        public string Name => _name;
        public AimSubMesh[] SubMeshes => _subMeshes;
        
        private readonly string _name;
        private AimSubMesh[] _subMeshes;
        
        public AimModel(string name)
        {
            _name = name;
        }
        
        public void ReadData(BinaryReader reader)
        {
            var subMeshesCount = reader.ReadInt32();
            reader.ReadBytes(0x40); // header
            
            _subMeshes = new AimSubMesh[subMeshesCount];
            for (var i = 0; i < subMeshesCount; i++)
            {
                var subMesh = new AimSubMesh();
                subMesh.ReadData(reader);
                
                _subMeshes[i] = subMesh;
            }
        }
    }
}