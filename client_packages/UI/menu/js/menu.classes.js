class PopupMenu {

	/**
	 *
	 * @param {string} title
	 * @param {string} subtitle
	 * @param {MenuItem[]} items
	 * @param {Number} limit
	 */
	constructor(title, subtitle, items, limit) {
		this.title = title || '';
		this.subtitle = subtitle || '&nbsp;';
		this.limit = limit || 7;
		this.items = items;
		this.stats = null;
		this.slider = null;
		this.colorPicker = null;
		this.grid = null;
		this.style = 'blue';
		this.onItemChange = null;

		this.index = 0;
	}

	// methods

	currentItem() {
		return this.items[this.index];
	}

	getStatByIndex(index) {
		return this.stats[index];
	}

	getStatByName(name) {
		this.stats.forEach(function(o) {
			if (o.name === name)
				return o;
		});

		return null;
	}

	setSliderValue(value) {
		if (this.slider)
			this.slider.value = value;
	}

	nextColorItem() {
		this.colorPicker.index++;

		if (this.colorPicker.index > this.colorPicker.colors.length - 1) {
			this.colorPicker.index = 0;
		}
	}

	prevColorItem() {
		this.colorPicker.index--;

		if (this.colorPicker.index < 0) {
			this.colorPicker.index = this.colorPicker.colors.length - 1;
		}
	}

	setGridXY(x, y) {
		this.grid.x = x;
		this.grid.y = y;
	}

	getGridXY() {
		return {
			x: this.grid.x,
			y: this.grid.y
		};
	}

	nextSelectionItem() {
		var item = this.currentItem();

		if (Object.prototype.toString.call(item.value) !== '[object Array]')
			return;

		item.index++;

		if (item.index > item.value.length - 1) {
			item.index = 0;
		}

		if ((typeof item.onChange) === 'function') {
			item.onChange(item.index, item.value[item.index]);
		}
	}

	prevSelectionItem() {
		var item = this.currentItem();

		if (Object.prototype.toString.call(item.value) !== '[object Array]')
			return;

		item.index--;

		if (item.index < 0) {
			item.index = item.value.length - 1;
		}

		if ((typeof item.onChange) === 'function') {
			item.onChange(item.index, item.value[item.index]);
		}
	}

	// modular methods

	Stats(stats) {
		this.stats = stats;
		return this;
	}

	Slider(name, units, value) {
		this.slider = {
			name: name || '&nbsp;',
			units: units || '%',
			value: value || 0
		};

		return this;
	}

	ColorPicker(name, colors) {
		this.colorPicker = {
			name: name,
			colors: colors,
			index: 0
		};

		return this;
	}

	XYGrid(x, y, top, bottom, left, right) {
		this.grid = {
			x: x || 0,
			y: y || 0,
			top: top,
			bottom: bottom,
			left: left,
			right: right
		};

		return this;
	}

	Style(style) {
		this.style = style || '';

		return this;
	}

	ActiveItemChanged(callback) {
		this.onItemChange = callback;

		return this;
	}
}

class MenuItem {

	/**
	 *
	 * @param {string} name
	 * @param {string|string[]} value
	 * @param {string} help
	 */
	constructor(name, value, help) {
		this.key   = name;
		this.value = value || null;
		this.help  =  help || null;
		this.submenu  = null;
		this.style    = null;
		this.action   = null;
		this.visible  = true;
		this.index    = 0;
		this.onChange = null;
	}

	Click(callback) {
		this.action = callback;

		return this;
	}

	Back() {
		this.submenu = true;

		return this;
	}

	SelectionChanged(callback) {
		this.onChange = callback;

		return this;
	}

	Style(style) {
		this.style = style;

		return this;
	}

	Submenu(menu) {
		this.submenu = menu;

		return this;
	}
}

class MenuStatItem {

	/**
	 *
	 * @param {string} name
	 * @param {Number} value
	 * @param {Number} levels
	 * @param {Number} width
	 */
	constructor(name, value, levels, width) {
		this.name = name || '';
		this.value = value || 0;
		this.levels = levels || 1;
		this.width = width || 160;
	}
}