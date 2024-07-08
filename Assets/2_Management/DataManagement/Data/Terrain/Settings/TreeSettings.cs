public class BiomeTreeType
{
    public float chance;
    public TreeType treeType;

    public BiomeTreeType()
    {

    }

    public BiomeTreeType(float chance, TreeType treeType)
    {
        this.chance = chance;
        this.treeType = treeType;
    }
}
public class TreeType
{
    public TreeTypes tree;
    public float minDistance;
}