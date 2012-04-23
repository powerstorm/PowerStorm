require 'test_helper'

class MetersControllerTest < ActionController::TestCase
  setup do
    @meter = meters(:one)
  end

  test "should get index" do
    get :index
    assert_response :success
    assert_not_nil assigns(:meters)
  end

  test "should get new" do
    get :new
    assert_response :success
  end

  test "should create meter" do
    assert_difference('Meter.count') do
      post :create, :meter => @meter.attributes
    end

    assert_redirected_to meter_path(assigns(:meter))
  end

  test "should show meter" do
    get :show, :id => @meter.to_param
    assert_response :success
  end

  test "should get edit" do
    get :edit, :id => @meter.to_param
    assert_response :success
  end

  test "should update meter" do
    put :update, :id => @meter.to_param, :meter => @meter.attributes
    assert_redirected_to meter_path(assigns(:meter))
  end

  test "should destroy meter" do
    assert_difference('Meter.count', -1) do
      delete :destroy, :id => @meter.to_param
    end

    assert_redirected_to meters_path
  end
end
