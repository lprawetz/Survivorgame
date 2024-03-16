extends TileMap

var moisture = FastNoiseLite.new()
var temperature = FastNoiseLite.new()
var altitude = FastNoiseLite.new()
var mapCenter = Vector2(0,0)
var width = 32
var height = 32
var tileSize = Vector2(62, 62)  # Adjusted tile size

@onready var player = get_parent().get_child(1)
																			 
func _ready():
	moisture.seed = randi()
	temperature.seed = randi()
	altitude.seed = randi()

func _process(delta):
	generate_chunk(player.position)

func generate_chunk(position):
	var tile_pos = local_to_map(position)
	for x in range(width):
		for y in range(height):
			var moist = moisture.get_noise_2d(tile_pos.x - width / 2 + x, tile_pos.y - height / 2 + y)*10
			var temp = temperature.get_noise_2d(tile_pos.x - width / 2 + x, tile_pos.y - height / 2 + y)*10
			var alt = altitude.get_noise_2d(tile_pos.x - width / 2 + x, tile_pos.y - height / 2 + y)*10

			# Calculate pixel position based on tile size
			var pixel_x = (tile_pos.x - width / 2 + x) * tileSize.x
			var pixel_y = (tile_pos.y - height / 2 + y) * tileSize.y

			# Set the tile using pixel position
			set_cell(0, Vector2i(pixel_x / 16, pixel_y / 16), 66, Vector2(round((moist + 10) / 2.5), round((temp + 10) / 2.5)))  # Assuming the source identifier is 66
##set_cell(Vector2i(pixel_x / tileSize.x, pixel_y / tileSize.y), 66, Vector2i(round((moist + 20) / 8), round((temp + 20) / 8)))
