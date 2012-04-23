require 'test_helper'

class ElectricityReadingsControllerTest < ActionController::TestCase
  setup do
    @electricity_reading = electricity_readings(:one)
  end

  test "should get index" do
    get :index
    assert_response :success
    assert_not_nil assigns(:electricity_readings)
  end

  test "should get new" do
    get :new
    assert_response :success
  end

  test "should create electricity_reading" do
    assert_difference('ElectricityReading.count') do
      post :create, :electricity_reading => @electricity_reading.attributes
    end

    assert_redirected_to electricity_reading_path(assigns(:electricity_reading))
  end

  test "should show electricity_reading" do
    get :show, :id => @electricity_reading.to_param
    assert_response :success
  end

  test "should get edit" do
    get :edit, :id => @electricity_reading.to_param
    assert_response :success
  end

  test "should update electricity_reading" do
    put :update, :id => @electricity_reading.to_param, :electricity_reading => @electricity_reading.attributes
    assert_redirected_to electricity_reading_path(assigns(:electricity_reading))
  end

  test "should destroy electricity_reading" do
    assert_difference('ElectricityReading.count', -1) do
      delete :destroy, :id => @electricity_reading.to_param
    end

    assert_redirected_to electricity_readings_path
  end
end
