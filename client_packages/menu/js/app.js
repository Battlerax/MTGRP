function LoadVue(menuInfo)
{
	var app = new Vue({
		el: '#container',
		data: {
			menu: menuInfo
		}
	});
}