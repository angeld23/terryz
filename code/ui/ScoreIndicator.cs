using System.Linq;
using Sandbox;
using Sandbox.UI;

public class ScoreIndicator : Panel
{

	public Label Label { get; protected set; }
	public float Score { get; set; }
	public Vector2[] BezierPoints { get; protected set; }
	public float Lifetime { get; set; }
	private readonly float _startTime;

	public ScoreIndicator ()
	{
		StyleSheet.Load( "styles/_scoreIndicator.scss" );

		SetClass( "scoreIndicator", true );

		Label = AddChild<Label>(  );
		Label.Text = "+0";
		Label.SetClass( "scoreIndicatorLabel", true);

		_startTime = Time.Now;
		Lifetime = 2;

		float jumpAmount = 0.2f;
		Vector2 origin = new(0.5f, 0.5f);
		Vector2 endPoint = new ( Rand.Float( 0.25f, 0.75f ), 1.1f );
		BezierPoints = new Vector2[]
		{
			origin, 
			new ( origin.x, origin.y + jumpAmount ), 
			new ( endPoint.x, origin.y + jumpAmount ), 
			endPoint
		};

		Log.Info( BezierPoints[0] );
		Log.Info( BezierPoints[1] );
		Log.Info( BezierPoints[2] );
		Log.Info( BezierPoints[3] );

		//Style.BackgroundColor = Color.Red;
		Style.Dirty();
		Label.Style.Dirty();
		
		Label.Style.Position = PositionMode.Absolute;
	}

	public override void Tick ()
	{
		var progress = (Time.Now - _startTime).LerpInverse( 0, Lifetime );
		var result = Bezier.GetPoint(progress, BezierPoints[0], BezierPoints[1], BezierPoints[2], BezierPoints[3]);
		Style.Left = Length.ViewWidth(result.x * 100);
		Style.Top = Length.ViewHeight(result.y * 100);

		Label.Text = "+" + Score;
		Log.Info( result.x );
		Log.Info( result.y );
		Log.Info( "" );

		if ( progress >= 1 )
		{
			Delete();
		}
	}
}
