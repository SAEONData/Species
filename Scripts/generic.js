var global = {};
global.reloadFunction = null;
global.onLoadView = null;
global.onLoadEdit = null;
global.onSaveFunc = null;
global.filterText = "";
global.postEdit = null;


global.selectedTaxon = null;

function getSelectedTaxon() {
	return global.selectedTaxon;
}

function doTaxon(recs) {
	global.records = recs;
	if (recs == null || recs.length == 0) {
		$('#results').html('No results found');
		$('#details').html('No results found');
		return;
	}

	var code = '<div class="list-group">';
	for (var i = 0; i < recs.length; i++) {
		var rec = recs[i];
		code += '<a href="#" id="rec' + i + '" class="list-group-item sresult">' + rec.vernacular + ': ' + rec.scientificname + '</a>';
	}
	code += '</div>';
	$('#results').html(code);

	$('.sresult').off();
	$('.sresult').click(function () {
		var id = this.id.substr(3);
		showRecord(id);
	});

	$('#results').scrollTop(0);
	showRecord(0);
}


function findTaxon(text) {
	var url = 'worms.ashx?name=' + escape(text);
	ajaxindicatorstart('Loading...');

	jQuery.getJSON(url, null, doTaxon)
	  .fail(function () {
			doTaxon(null);
	  		alert('Failed to retreive data');
	  })
	  .always(function () {
	  		ajaxindicatorstop();
	  });


}

function showRecord(id) {
	$('.sresult').removeClass('active');
	$('#rec' + id).addClass('active');

	var rec = global.records[id];

	var code = '<table cellpadding=4>';

	// Common name
	code += '<tr>';
	code += '<td><strong>Common name: </strong></td>';
	code += '<td>' + rec.vernacular + '</td>';
	code += '</tr>';

	// Scientific Name
	code += '<tr>';
	code += '<td><strong>Scientific Name: </strong></td>';
	code += '<td>' + rec.scientificname + '</td>';
	code += '</tr>';

	// LSID
	code += '<tr>';
	code += '<td><strong>LSID: </strong></td>';
	code += '<td>' + rec.lsid + '</td>';
	code += '</tr>';

	// AphiaID
	code += '<tr>';
	code += '<td><strong>AphiaID: </strong></td>';
	code += '<td>' + rec.AphiaID + '</td>';
	code += '</tr>';

	var taxons = rec.taxon;
	for (var i = 0; i < taxons.length; i++) {
		var taxon = taxons[i];
		if (taxon.rank != null) {
			code += '<tr>';
			code += '<td><strong>' + taxon.rank + ': </strong></td>';
			code += '<td>' + taxon.name + '</td>';
			code += '</tr>';
		}
	}


	code += '</table>';

	$('#details').html(code);

	global.selectedTaxon = rec.vernacular + ': ' + rec.scientificname + ' (' + rec.AphiaID + ')';
	global.selectedCommon = rec.vernacular;
	global.selectedSpecies = rec.scientificname;
}

function getRowID(id) {
	return parseInt(id.substring(3));
}

function getPageName() {
	return location.pathname.substring(location.pathname.lastIndexOf("/") + 1);
}

function setFieldData(code, view, id) {
	debugger;
	global.fields = new Array();
	eval('data = ' + code);
	for (var i = 0; i < data.length; i++) {
		var field = data[i];
		global.fields.push(field.name);
		if (view == true)
			$('#vf' + field.name).html(field.text);
		else {
			var control = $('#ef' + field.name);

			if (control.hasClass('datepicker')) {
				if (field.value == null || field.value == 'undefined' || field.value == "") {
					var date = new Date();
					control.datepicker('setDate', date);
				}
				else {
					var date = new Date(field.value);
					control.datepicker('setDate', date);
				}
			}
			else if (control.hasClass('photobox')) {
				$('.photobox').attr('src', 'photobox.aspx?id=' + id + '&photo=' + field.value);
			}
			else if (control.hasClass('taxonList')) {
				alert('1');
			}
			else {
				if (field.value == null || field.value == 'undefined')
					control.val('');
				else
					control.val(field.value);
			}
		}
	}

	if ($('.taxonList').length) {
		$('.taxonList').attr('src', 'taxonlist.aspx?id=' + id + '&view=' + view);
	}

}

function initControls() {

	$(".viewButton").off();
	$(".viewButton").click(function (event) {
		var target = $(this).attr('target');
		event.stopPropagation();
		var id = getRowID(this.id);
		global.recordID = id;
		var page = getPageName();
		$.ajax({
			url: page + "?mode=loadrec&id=" + id + "&rid=" + Math.random(),
		}).done(function (code) {
			setFieldData(code, true, id);
			if (global.onLoadView != null)
				global.onLoadView(id);
			$('#viewDialog' + target).modal();
			$('#viewDialog' + target).draggable({
				handle: ".modal-header"
			});

			$('#viewDialog' + target).on('shown.modal', function () {
				alert('center');
				var initModalHeight = $('#modal-dialog').outerHeight(); //give an id to .mobile-dialog
				var userScreenHeight = $(document).outerHeight();
				if (initModalHeight > userScreenHeight) {
					$('#modal-dialog').css('overflow', 'auto'); //set to overflow if no fit
				} else {
					$('#modal-dialog').css('margin-top',
					(userScreenHeight / 2) - (initModalHeight / 2)); //center it if it does fit
				}
			});


		});
		return false;
	});

	$(".editButton").off();
	$(".editButton").click(function (event) {
		var target = $(this).attr('target');
		event.stopPropagation();
		var id = getRowID(this.id);
		global.recordID = id;
		var page = getPageName();
		$('#editDialogTitle' + target).html('Edit ' + dbType);
		$.ajax({
			url: page + "?mode=loadrec&id=" + id + "&rid=" + Math.random(),
		}).done(function (code) {
			setFieldData(code, false, id);
			if (global.onLoadEdit != null)
				global.onLoadEdit(id);
			$('#editDialog' + target).modal();
			$('#editDialog' + target).draggable({
				handle: ".modal-header"
			});

			if (global.postEdit != null)
				global.postEdit(id, code);
		});
		return false;
	});

	$(".deleteButton").off();
	$(".deleteButton").click(function (event) {
		var target = $(this).attr('target');
		var id = getRowID(this.id);
		global.recordID = id;
		event.stopPropagation();
		$('#deleteDialog' + target).modal();
		$('#deleteDialog' + target).draggable({
			handle: ".modal-header"
		});

		return false;
	});

	$(".addButton").off();
	$(".addButton").click(function (event) {
		var target = $(this).attr('target');
		event.stopPropagation();
		var id = 0;
		global.recordID = id;
		var page = getPageName();
		$('#editDialogTitle' + target).html('Add ' + dbType);
		$.ajax({
			url: page + "?mode=loadrec&id=" + id + "&rid=" + Math.random(),
		}).done(function (code) {
			setFieldData(code, false, id);
			if (global.onLoadEdit != null)
				global.onLoadEdit(id);
			$('#editDialog' + target).modal();
			$('#editDialog' + target).draggable({
				handle: ".modal-header"
			});

		});
		return false;
	});

	$(".textFilter").off();
	$(".textFilter").click(function () {
		global.filterText = escape($('#textFilter').val());
		loadTable();
		return false;
	});



	$("#buttonSave").off();
	$('#buttonSave').click(function () {
		debugger;

		var target = $(this).attr('target');
		

		console.log('save: ' + target);


		// alert(target);


		var params = "";
		for (var i = 0; i < global.fields.length; i++) {
			var field = global.fields[i];
			var control = $('#ef' + field);
			var value = control.val();

			if (control.hasClass('datepicker')) {
				var date = control.datepicker('getDate');
				value = date.format("mm-dd-yy");
			}

			if (control.hasClass('photobox')) {
				window.frames['photobox'].saveTags();
			}
			else {
				params += "&" + field + "=" + escape(value);
			}
		}

		if ($('#taxonList').length) {
			var value = window.frames["taxonList"].getTaxons();
			params += "&taxons=" + escape(value); 
		}

		if (global.onSaveFunc != null)
			params += global.onSaveFunc();

		var page = getPageName();
		var url = page + "?mode=save&id=" + global.recordID + params + "&rid=" + Math.random();
		$.ajax({
			url: url,
		}).done(function (code) {
			global.target = target;
			loadTable(closeEditBox, global.filter);
		});
	});

	$("#buttonDelete").off();
	$('#buttonDelete').click(function () {
		var target = $(this).attr('target');
		var page = getPageName();
		var url = page + "?mode=delete&id=" + global.recordID + "&rid=" + Math.random();
		$.ajax({
			url: url,
		}).done(function (code) {
			global.target = target;
			loadTable(closeDeleteBox, global.filter);
		});
	});


	$('.btnShowAll').off();
	$('.btnShowAll').click(function () {
		ajaxindicatorstart('Loading...');
		location.reload(false);
	});


	$('.btnExcelExport').off();
	$('.btnExcelExport').click(function () {
		if (global.reportName == null || global.reportName == '') {
			global.reportName = 'export.csv';
			global.seperator = ',';
		}
		getItemName("Export to CSV", "File Name", global.reportName, global.seperator, function (name, seperator) {
			var lcase = name.toLowerCase();
			if (lcase.indexOf('.csv') == -1)
				name += '.csv';
			global.reportName = name;
			global.seperator = seperator;
			var url = getPageName() + "?mode=exportcsv&filter=" + global.filter + "&filterText=" + global.filterText + "&rid=" + Math.random();
			$('#saveframe').attr('src', "savecsv.aspx?url=" + escape(url) + "&name=" + escape(name));
		});
	});


	$(".taxonButton").off();
	$('.taxonButton').click(function () {

		var field = this.id.substring(2);
		global.taxonControl = $('#ef' + field);
		var text = global.taxonControl.val();
		$('#find').val(text);
		$('#results').html('');
		$('#details').html('');
		if (text != '' && text != null)
			findTaxon(text);
		$('#taxonDialog').modal();
		$('#taxonDialog').draggable({
			handle: ".modal-header"
		});

		return false;
	});


	$("#buttonTaxonSelect").off();
	$('#buttonTaxonSelect').click(function () {
		var taxon = getSelectedTaxon();
		if (taxon == null) {
			alert('Please select a species');
			return;
		}
		$('#taxonDialog').modal('toggle');
		$('#taxonDialog').draggable({
			handle: ".modal-header"
		});

		global.taxonControl.val(taxon);

		if ($('#effCommonName').val() == '')
			$('#effCommonName').val(global.selectedCommon);

		if ($('#effSpeciesName').val() == '')
			$('#effSpeciesName').val(global.selectedSpecies);

		return fals
	});


	$('.datepicker').datepicker({ autoclose: true });

	
	$('.timepickerControl').timepicker({ autoclose: true });

	var text = $('#find').val();
	if (text != '' && text != null)
		findTaxon(text);

	$(".findTaxonButton").off();
	$('.findTaxonButton').click(function () {
		var text = $('#find').val();
		findTaxon(text);
		return false;
	});



}

function closeEditBox() {
	$('#editDialog' + global.target).modal('toggle');
}

function closeDeleteBox() {
	$('#deleteDialog' + global.target).modal('toggle');
}

function loadDialogs() {
	var page = getPageName();
	var url = page + "?mode=loaddlg";
	$("#dialogsDiv").load(url, function () {

	});
}

function loadTable(retFunc, filterArg) {
	if (filterArg == null)
		filterArg = '';
	global.filter = filterArg;
	global.retFunc = retFunc;

	var file = getPageName();
	var url = file + "?mode=loadlist&filter=" + filterArg + "&filterText=" + global.filterText + "&rid=" + Math.random();
	ajaxindicatorstart('Loading..');
	$("#tableDiv").load(url, function () {
		ajaxindicatorstop();
		initControls();
		if (retFunc != null)
			retFunc();
		if (global.reloadFunction)
			global.reloadFunction();
	});
}

function loadPage(num) {
	filterArg = global.filter;
	retFunc = global.retFunc;
	var file = getPageName();
	var url = file + "?mode=loadlist&filter=" + filterArg + "&filterText=" + global.filterText + "&page=" + num + "&rid=" + Math.random();
	$("#tableDiv").load(url, function () {
		initControls();
		if (retFunc != null)
			retFunc();
		if (global.reloadFunction)
			global.reloadFunction();
	});
}

$(document).ready(function () {
	loadDialogs();
	loadTable();
});

function clearCombo(id) {
	$("#" + id).empty();
}

function addComboOption(id, val, text) {
	$('#' + id).append($('<option/>', {
		value: val,
		text: text
	}));
}

function enableControl(id, enable) {
	$('#' + id).attr('disabled', enable == false);
}

function getDatePickerDate(id) {
	var date = $('#' + id).datepicker('getDate');
	if (date == null || date == "" || date == "Invalid Date")
		return "";
	return date.format("mm-dd-yy");
}