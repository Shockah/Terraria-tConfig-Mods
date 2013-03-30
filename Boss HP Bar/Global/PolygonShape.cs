/*
source: http://stackoverflow.com/questions/7246459/creating-a-2d-polygon-in-xna
*/

public class PolygonShape {
	private GraphicsDevice graphicsDevice;
	private VertexPositionColorTexture[] vertices, triangulatedVertices;
	private bool triangulated;
	private int[] indexes;
	private Vector3 centerPoint;
	
	public PolygonShape(GraphicsDevice graphicsDevice, VertexPositionColorTexture[] vertices)
	{
		this.graphicsDevice = graphicsDevice;
		this.vertices = vertices;
		this.triangulated = false;

		triangulatedVertices = new VertexPositionColorTexture[vertices.Length * 3];
		indexes = new int[vertices.Length];
	}
	
	public VertexPositionColorTexture[] Triangulate()
	{
		calculateCenterPoint();
		setupIndexes();
		for (int i = 0; i < indexes.Length; i++) setupDrawableTriangle(indexes[i]);
		triangulated = true;
		return triangulatedVertices;
	}
	
	private void calculateCenterPoint()
	{
		float xCount = 0, yCount = 0;

		foreach (VertexPositionColorTexture vertice in vertices)
		{
			xCount += vertice.Position.X;
			yCount += vertice.Position.Y;
		}

		centerPoint = new Vector3(xCount / vertices.Length, yCount / vertices.Length, 0);
	}

	private void setupIndexes()
	{
		for (int i = 1; i < triangulatedVertices.Length; i = i + 3)
		{
			indexes[i / 3] = i - 1;
		}
	}

	private void setupDrawableTriangle(int index)
	{
		triangulatedVertices[index] = vertices[index / 3]; //No DividedByZeroException?...
		if (index / 3 != vertices.Length - 1)
			triangulatedVertices[index + 1] = vertices[(index / 3) + 1];
		else
			triangulatedVertices[index + 1] = vertices[0];
		triangulatedVertices[index + 2].Position = centerPoint;
	}

	public void Draw()
	{
		try
		{
			if (!triangulated) Triangulate();
			draw();
		}
		catch (Exception exception)
		{
			throw exception;
		}
	}

	private void draw()
	{
		graphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, triangulatedVertices, 0, vertices.Length);
	}
}